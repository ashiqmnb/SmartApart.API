using SmartApart.API.Common;
using SmartApart.API.DTOs.Complaints;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepo;
        private readonly IResidentRepository _residentRepo;
        private readonly ILogger<ComplaintService> _logger;

        // Allowed forward transitions — no skipping
        private static readonly Dictionary<ComplaintStatus, ComplaintStatus[]> AllowedTransitions = new()
        {
            [ComplaintStatus.Pending] = new[] { ComplaintStatus.UnderReview, ComplaintStatus.Closed },
            [ComplaintStatus.UnderReview] = new[] { ComplaintStatus.Resolved, ComplaintStatus.Closed },
            [ComplaintStatus.Resolved] = new[] { ComplaintStatus.Closed },
            [ComplaintStatus.Closed] = Array.Empty<ComplaintStatus>()
        };

        public ComplaintService(
            IComplaintRepository complaintRepo,
            IResidentRepository residentRepo,
            ILogger<ComplaintService> logger)
        {
            _complaintRepo = complaintRepo;
            _residentRepo = residentRepo;
            _logger = logger;
        }

        // ── Create Complaint ─────────────────────────────────────────

        public async Task<ComplaintDetailDto> CreateComplaintAsync(CreateComplaintRequestDto dto, Guid callerUserId)
        {
            try
            {
                var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                    ?? throw new AppException("Resident profile not found.", 404);

                var complaint = new Complaint
                {
                    Id = Guid.NewGuid(),
                    ResidentId = resident.Id,
                    Category = dto.Category,
                    ComplaintType = dto.ComplaintType,
                    Title = dto.Title,
                    Description = dto.Description,
                    Status = ComplaintStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _complaintRepo.AddAsync(complaint);
                await _complaintRepo.SaveChangesAsync();

                var saved = await _complaintRepo.GetByIdAsync(complaint.Id)
                    ?? throw new AppException("Failed to load created complaint.", 500);

                return MapToDetailDto(saved, callerRole: Role.Resident);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateComplaintAsync failed for user {UserId}", callerUserId);
                throw new AppException("Failed to create complaint.", 500);
            }
        }

        // ── Get All Complaints (Filtered) ────────────────────────────

        public async Task<PagedResult<ComplaintListDto>> GetAllComplaintsAsync(
            ComplaintFilterRequestDto filter,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                Guid? residentFilterId = null;

                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    residentFilterId = resident.Id;
                }

                ComplaintStatus? statusFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    if (!Enum.TryParse<ComplaintStatus>(filter.Status, true, out var parsedStatus))
                        throw new AppException("Invalid status filter.", 400);
                    statusFilter = parsedStatus;
                }

                ComplaintCategory? categoryFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    if (!Enum.TryParse<ComplaintCategory>(filter.Category, true, out var parsedCategory))
                        throw new AppException("Invalid category filter.", 400);
                    categoryFilter = parsedCategory;
                }

                var (complaints, totalCount) = await _complaintRepo.GetAllAsync(
                    residentFilterId, statusFilter, categoryFilter, filter.Page, filter.PageSize);

                return new PagedResult<ComplaintListDto>
                {
                    Items = complaints.Select(c => MapToListDto(c, callerRole)).ToList(),
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllComplaintsAsync failed");
                throw new AppException("Failed to retrieve complaints.", 500);
            }
        }

        // ── Get Complaint By Id ──────────────────────────────────────

        public async Task<ComplaintDetailDto> GetComplaintByIdAsync(Guid complaintId, Guid callerUserId, Role callerRole)
        {
            try
            {
                var complaint = await _complaintRepo.GetByIdAsync(complaintId)
                    ?? throw new AppException("Complaint not found.", 404);

                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    if (complaint.ResidentId != resident.Id)
                        throw new AppException("You are not authorized to view this complaint.", 403);
                }

                return MapToDetailDto(complaint, callerRole);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetComplaintByIdAsync failed for {ComplaintId}", complaintId);
                throw new AppException("Failed to retrieve complaint.", 500);
            }
        }

        // ── Add Image ─────────────────────────────────────────────────

        public async Task<ComplaintImageDto> AddImageAsync(Guid complaintId, string imageUrl, Guid callerUserId, Role callerRole)
        {
            try
            {
                var complaint = await _complaintRepo.GetByIdAsync(complaintId)
                    ?? throw new AppException("Complaint not found.", 404);

                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    if (complaint.ResidentId != resident.Id)
                        throw new AppException("You are not authorized to add images to this complaint.", 403);
                }

                var image = new ComplaintImage
                {
                    Id = Guid.NewGuid(),
                    ComplaintId = complaintId,
                    ImageUrl = imageUrl,
                    UploadedAt = DateTime.UtcNow
                };

                await _complaintRepo.AddImageAsync(image);
                await _complaintRepo.SaveChangesAsync();

                return new ComplaintImageDto
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    UploadedAt = image.UploadedAt
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddImageAsync failed for complaint {ComplaintId}", complaintId);
                throw new AppException("Failed to upload image.", 500);
            }
        }

        // ── Update Status (Admin) ────────────────────────────────────

        public async Task<ComplaintDetailDto> UpdateStatusAsync(Guid complaintId, UpdateComplaintStatusRequestDto dto)
        {
            try
            {
                var complaint = await _complaintRepo.GetByIdAsync(complaintId)
                    ?? throw new AppException("Complaint not found.", 404);

                if (!Enum.TryParse<ComplaintStatus>(dto.Status, true, out var newStatus))
                    throw new AppException("Invalid status value.", 400);

                ValidateTransition(complaint.Status, newStatus);

                complaint.Status = newStatus;
                complaint.UpdatedAt = DateTime.UtcNow;

                if (newStatus == ComplaintStatus.Resolved)
                {
                    complaint.ResolvedAt = DateTime.UtcNow;
                    complaint.ResolutionNote = dto.ResolutionNote;
                }
                else if (!string.IsNullOrWhiteSpace(dto.ResolutionNote))
                {
                    complaint.ResolutionNote = dto.ResolutionNote;
                }

                await _complaintRepo.UpdateAsync(complaint);
                await _complaintRepo.SaveChangesAsync();

                // TODO: FCM to resident (Phase 1.7)
                _logger.LogInformation("Complaint {ComplaintId} status updated to {Status}", complaintId, newStatus);

                var saved = await _complaintRepo.GetByIdAsync(complaintId)
                    ?? throw new AppException("Failed to load updated complaint.", 500);

                // Admin always sees full detail (audit trail), so pass Admin role here
                return MapToDetailDto(saved, Role.Admin);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateStatusAsync failed for {ComplaintId}", complaintId);
                throw new AppException("Failed to update complaint status.", 500);
            }
        }

        // ── Status Transition Guard ──────────────────────────────────

        private static void ValidateTransition(ComplaintStatus current, ComplaintStatus target)
        {
            if (current == target)
                throw new AppException($"Complaint is already in {current} status.", 422);

            if (!AllowedTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
                throw new AppException($"Invalid status transition from {current} to {target}.", 422);
        }

        // ── Mappers (Anonymous Masking) ──────────────────────────────

        private static ComplaintDetailDto MapToDetailDto(Complaint c, Role callerRole)
        {
            var isAnonymous = c.ComplaintType == ComplaintType.Anonymous;
            var maskIdentity = isAnonymous && callerRole != Role.Admin;

            return new ComplaintDetailDto
            {
                Id = c.Id,
                ResidentName = maskIdentity ? "Anonymous" : (c.Resident?.User?.FullName ?? string.Empty),
                ApartmentNumber = maskIdentity ? null : c.Resident?.ApartmentNumber,
                Block = maskIdentity ? null : c.Resident?.Block,
                Category = c.Category.ToString(),
                ComplaintType = c.ComplaintType.ToString(),
                Title = c.Title,
                Description = c.Description,
                Status = c.Status.ToString(),
                ResolutionNote = c.ResolutionNote,
                ResolvedAt = c.ResolvedAt,
                ImageUrls = c.Images?.Select(i => i.ImageUrl).ToList() ?? new(),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }

        private static ComplaintListDto MapToListDto(Complaint c, Role callerRole)
        {
            var isAnonymous = c.ComplaintType == ComplaintType.Anonymous;
            var maskIdentity = isAnonymous && callerRole != Role.Admin;

            return new ComplaintListDto
            {
                Id = c.Id,
                ResidentName = maskIdentity ? "Anonymous" : (c.Resident?.User?.FullName ?? string.Empty),
                Category = c.Category.ToString(),
                ComplaintType = c.ComplaintType.ToString(),
                Title = c.Title,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            };
        }
    }

}
