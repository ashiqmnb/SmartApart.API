using SmartApart.API.Common;
using SmartApart.API.DTOs.Maintenance;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IMaintenanceRepository _maintenanceRepo;
        private readonly IResidentRepository _residentRepo;
        private readonly ILogger<MaintenanceService> _logger;

        // Allowed forward transitions — no skipping
        private static readonly Dictionary<MaintenanceStatus, MaintenanceStatus[]> AllowedTransitions = new()
        {
            [MaintenanceStatus.Open] = new[] { MaintenanceStatus.Assigned, MaintenanceStatus.Cancelled },
            [MaintenanceStatus.Assigned] = new[] { MaintenanceStatus.InProgress, MaintenanceStatus.Cancelled },
            [MaintenanceStatus.InProgress] = new[] { MaintenanceStatus.Completed, MaintenanceStatus.Cancelled },
            [MaintenanceStatus.Completed] = Array.Empty<MaintenanceStatus>(),
            [MaintenanceStatus.Cancelled] = Array.Empty<MaintenanceStatus>()
        };

        public MaintenanceService(
            IMaintenanceRepository maintenanceRepo,
            IResidentRepository residentRepo,
            ILogger<MaintenanceService> logger)
        {
            _maintenanceRepo = maintenanceRepo;
            _residentRepo = residentRepo;
            _logger = logger;
        }

        // ── Create Request ───────────────────────────────────────────

        public async Task<MaintenanceDetailDto> CreateRequestAsync(CreateMaintenanceRequestDto dto, Guid callerUserId)
        {
            try
            {
                var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                    ?? throw new AppException("Resident profile not found.", 404);

                var request = new MaintenanceRequest
                {
                    Id = Guid.NewGuid(),
                    ResidentId = resident.Id,
                    Category = dto.Category,
                    Priority = dto.Priority,
                    Title = dto.Title,
                    Description = dto.Description,
                    Status = MaintenanceStatus.Open,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _maintenanceRepo.AddAsync(request);
                await _maintenanceRepo.SaveChangesAsync();

                var saved = await _maintenanceRepo.GetByIdAsync(request.Id)
                    ?? throw new AppException("Failed to load created request.", 500);

                return MapToDetailDto(saved);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateRequestAsync failed for user {UserId}", callerUserId);
                throw new AppException("Failed to create maintenance request.", 500);
            }
        }

        // ── Get All Requests (Filtered) ──────────────────────────────

        public async Task<PagedResult<MaintenanceListDto>> GetAllRequestsAsync(
            MaintenanceFilterRequestDto filter,
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

                MaintenanceStatus? statusFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    if (!Enum.TryParse<MaintenanceStatus>(filter.Status, true, out var parsedStatus))
                        throw new AppException("Invalid status filter.", 400);
                    statusFilter = parsedStatus;
                }

                MaintenanceCategory? categoryFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.Category))
                {
                    if (!Enum.TryParse<MaintenanceCategory>(filter.Category, true, out var parsedCategory))
                        throw new AppException("Invalid category filter.", 400);
                    categoryFilter = parsedCategory;
                }

                Priority? priorityFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.Priority))
                {
                    if (!Enum.TryParse<Priority>(filter.Priority, true, out var parsedPriority))
                        throw new AppException("Invalid priority filter.", 400);
                    priorityFilter = parsedPriority;
                }

                var (requests, totalCount) = await _maintenanceRepo.GetAllAsync(
                    residentFilterId, statusFilter, categoryFilter, priorityFilter, filter.Page, filter.PageSize);

                return new PagedResult<MaintenanceListDto>
                {
                    Items = requests.Select(MapToListDto).ToList(),
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllRequestsAsync failed");
                throw new AppException("Failed to retrieve maintenance requests.", 500);
            }
        }

        // ── Get Request By Id ────────────────────────────────────────

        public async Task<MaintenanceDetailDto> GetRequestByIdAsync(Guid requestId, Guid callerUserId, Role callerRole)
        {
            try
            {
                var request = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Maintenance request not found.", 404);

                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    if (request.ResidentId != resident.Id)
                        throw new AppException("You are not authorized to view this request.", 403);
                }

                return MapToDetailDto(request);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRequestByIdAsync failed for {RequestId}", requestId);
                throw new AppException("Failed to retrieve maintenance request.", 500);
            }
        }

        // ── Add Image ─────────────────────────────────────────────────

        public async Task<MaintenanceImageDto> AddImageAsync(Guid requestId, string imageUrl, Guid callerUserId, Role callerRole)
        {
            try
            {
                var request = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Maintenance request not found.", 404);

                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    if (request.ResidentId != resident.Id)
                        throw new AppException("You are not authorized to add images to this request.", 403);
                }

                var image = new MaintenanceImage
                {
                    Id = Guid.NewGuid(),
                    RequestId = requestId,
                    ImageUrl = imageUrl,
                    UploadedAt = DateTime.UtcNow
                };

                await _maintenanceRepo.AddImageAsync(image);
                await _maintenanceRepo.SaveChangesAsync();

                return new MaintenanceImageDto
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    UploadedAt = image.UploadedAt
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddImageAsync failed for request {RequestId}", requestId);
                throw new AppException("Failed to upload image.", 500);
            }
        }

        // ── Assign Request ───────────────────────────────────────────

        public async Task<MaintenanceDetailDto> AssignRequestAsync(Guid requestId, AssignMaintenanceRequestDto dto)
        {
            try
            {
                var request = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Maintenance request not found.", 404);

                ValidateTransition(request.Status, MaintenanceStatus.Assigned);

                request.AssignedTo = dto.AssignedTo;
                request.AssignedAt = DateTime.UtcNow;
                request.Status = MaintenanceStatus.Assigned;
                request.UpdatedAt = DateTime.UtcNow;

                await _maintenanceRepo.UpdateAsync(request);
                await _maintenanceRepo.SaveChangesAsync();

                // TODO: FCM to resident (Phase 1.7)
                _logger.LogInformation("Maintenance request {RequestId} assigned to {StaffId}", requestId, dto.AssignedTo);

                var saved = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Failed to load updated request.", 500);

                return MapToDetailDto(saved);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssignRequestAsync failed for {RequestId}", requestId);
                throw new AppException("Failed to assign maintenance request.", 500);
            }
        }

        // ── Update Status ────────────────────────────────────────────

        public async Task<MaintenanceDetailDto> UpdateStatusAsync(Guid requestId, UpdateMaintenanceStatusRequestDto dto)
        {
            try
            {
                var request = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Maintenance request not found.", 404);

                if (!Enum.TryParse<MaintenanceStatus>(dto.Status, true, out var newStatus))
                    throw new AppException("Invalid status value.", 400);

                ValidateTransition(request.Status, newStatus);

                request.Status = newStatus;
                request.UpdatedAt = DateTime.UtcNow;

                if (newStatus == MaintenanceStatus.Completed)
                {
                    request.ResolvedAt = DateTime.UtcNow;
                    request.ResolutionNote = dto.ResolutionNote;
                }
                else if (!string.IsNullOrWhiteSpace(dto.ResolutionNote))
                {
                    request.ResolutionNote = dto.ResolutionNote;
                }

                await _maintenanceRepo.UpdateAsync(request);
                await _maintenanceRepo.SaveChangesAsync();

                // TODO: FCM to resident (Phase 1.7)
                _logger.LogInformation("Maintenance request {RequestId} status updated to {Status}", requestId, newStatus);

                var saved = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Failed to load updated request.", 500);

                return MapToDetailDto(saved);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateStatusAsync failed for {RequestId}", requestId);
                throw new AppException("Failed to update request status.", 500);
            }
        }

        // ── Cancel Request (Resident, own Open only) ─────────────────

        public async Task CancelRequestAsync(Guid requestId, Guid callerUserId)
        {
            try
            {
                var request = await _maintenanceRepo.GetByIdAsync(requestId)
                    ?? throw new AppException("Maintenance request not found.", 404);

                var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                    ?? throw new AppException("Resident profile not found.", 404);

                if (request.ResidentId != resident.Id)
                    throw new AppException("You are not authorized to cancel this request.", 403);

                if (request.Status != MaintenanceStatus.Open)
                    throw new AppException("Only Open requests can be cancelled.", 422);

                request.Status = MaintenanceStatus.Cancelled;
                request.UpdatedAt = DateTime.UtcNow;

                await _maintenanceRepo.UpdateAsync(request);
                await _maintenanceRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelRequestAsync failed for {RequestId}", requestId);
                throw new AppException("Failed to cancel maintenance request.", 500);
            }
        }

        // ── Status Transition Guard ──────────────────────────────────

        private static void ValidateTransition(MaintenanceStatus current, MaintenanceStatus target)
        {
            if (current == target)
                throw new AppException($"Request is already in {current} status.", 422);

            if (!AllowedTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(target))
                throw new AppException($"Invalid status transition from {current} to {target}.", 422);
        }

        // ── Mappers ──────────────────────────────────────────────────

        private static MaintenanceDetailDto MapToDetailDto(MaintenanceRequest mr) => new()
        {
            Id = mr.Id,
            ResidentId = mr.ResidentId,
            ResidentName = mr.Resident?.User?.FullName ?? string.Empty,
            ApartmentNumber = mr.Resident?.ApartmentNumber ?? string.Empty,
            Block = mr.Resident?.Block ?? string.Empty,
            Category = mr.Category.ToString(),
            Priority = mr.Priority.ToString(),
            Title = mr.Title,
            Description = mr.Description,
            Status = mr.Status.ToString(),
            AssignedTo = mr.AssignedTo,
            AssignedStaffName = mr.AssignedStaff?.FullName,
            AssignedAt = mr.AssignedAt,
            ResolvedAt = mr.ResolvedAt,
            ResolutionNote = mr.ResolutionNote,
            ImageUrls = mr.Images?.Select(i => i.ImageUrl).ToList() ?? new(),
            CreatedAt = mr.CreatedAt,
            UpdatedAt = mr.UpdatedAt
        };

        private static MaintenanceListDto MapToListDto(MaintenanceRequest mr) => new()
        {
            Id = mr.Id,
            ResidentName = mr.Resident?.User?.FullName ?? string.Empty,
            ApartmentNumber = mr.Resident?.ApartmentNumber ?? string.Empty,
            Block = mr.Resident?.Block ?? string.Empty,
            Category = mr.Category.ToString(),
            Priority = mr.Priority.ToString(),
            Title = mr.Title,
            Status = mr.Status.ToString(),
            CreatedAt = mr.CreatedAt
        };
    }
}