using SmartApart.API.Common;
using SmartApart.API.DTOs.Visitors;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly IVisitorRepository _visitorRepo;
        private readonly IResidentRepository _residentRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<VisitorService> _logger;

        public VisitorService(
            IVisitorRepository visitorRepo,
            IResidentRepository residentRepo,
            ILogger<VisitorService> logger,
            INotificationService notificationService)
        {
            _visitorRepo = visitorRepo;
            _residentRepo = residentRepo;
            _logger = logger;
            _notificationService = notificationService;
        }



        // ── Register Visitor ─────────────────────────────────────────
        public async Task<VisitorDetailDto> RegisterVisitorAsync(RegisterVisitorRequestDto dto, Guid securityUserId)
        {
            try
            {
                var resident = await _residentRepo.GetByIdAsync(dto.ResidentId)
                    ?? throw new AppException("Resident not found.", 404);

                var visitor = new Visitor
                {
                    Id = Guid.NewGuid(),
                    ResidentId = dto.ResidentId,
                    SecurityId = securityUserId,
                    VisitorName = dto.VisitorName,
                    VisitorPhone = dto.VisitorPhone,
                    Purpose = dto.Purpose,
                    VehicleNumber = dto.VehicleNumber,
                    EntryTime = DateTime.UtcNow,
                    ApprovalStatus = VisitorStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _visitorRepo.AddAsync(visitor);
                await _visitorRepo.SaveChangesAsync();

                // TODO: Dispatch FCM to resident (Phase 1.7) — type: VisitorApproval, referenceId: visitor.Id
                _logger.LogInformation("Visitor {VisitorId} registered for resident {ResidentId}", visitor.Id, dto.ResidentId);
                await _notificationService.CreateAndSendAsync(
                    visitor.Resident.UserId,
                    "New Visitor Request",
                    $"{visitor.VisitorName} is here to see you. Purpose: {visitor.Purpose}",
                    "VisitorApproval",
                    visitor.Id);

                var saved = await _visitorRepo.GetByIdAsync(visitor.Id)
                    ?? throw new AppException("Failed to load registered visitor.", 500);

                return MapToDetailDto(saved);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterVisitorAsync failed for resident {ResidentId}", dto.ResidentId);
                throw new AppException("Failed to register visitor.", 500);
            }
        }



        // ── Get All Visitors (Filtered) ──────────────────────────────
        public async Task<PagedResult<VisitorListDto>> GetAllVisitorsAsync(
            VisitorFilterRequestDto filter,
            Guid callerUserId,
            Role callerRole)
        {
            try
            {
                Guid? residentFilterId = null;

                // Resident only sees their own unit's visitors
                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    residentFilterId = resident.Id;
                }

                VisitorStatus? statusFilter = null;
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    if (!Enum.TryParse<VisitorStatus>(filter.Status, true, out var parsedStatus))
                        throw new AppException("Invalid status filter.", 400);
                    statusFilter = parsedStatus;
                }

                var (visitors, totalCount) = await _visitorRepo.GetAllAsync(
                    residentFilterId, statusFilter, filter.From, filter.To, filter.Page, filter.PageSize);

                return new PagedResult<VisitorListDto>
                {
                    Items = visitors.Select(MapToListDto).ToList(),
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllVisitorsAsync failed");
                throw new AppException("Failed to retrieve visitors.", 500);
            }
        }



        // ── Get Visitor By Id ─────────────────────────────────────────
        public async Task<VisitorDetailDto> GetVisitorByIdAsync(Guid visitorId, Guid callerUserId, Role callerRole)
        {
            try
            {
                var visitor = await _visitorRepo.GetByIdAsync(visitorId)
                    ?? throw new AppException("Visitor record not found.", 404);

                if (callerRole == Role.Resident)
                {
                    var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                        ?? throw new AppException("Resident profile not found.", 404);

                    if (visitor.ResidentId != resident.Id)
                        throw new AppException("You are not authorized to view this visitor record.", 403);
                }

                return MapToDetailDto(visitor);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetVisitorByIdAsync failed for {VisitorId}", visitorId);
                throw new AppException("Failed to retrieve visitor.", 500);
            }
        }



        // ── Approve Visitor ──────────────────────────────────────────
        public async Task<VisitorDetailDto> ApproveVisitorAsync(Guid visitorId, Guid callerUserId)
        {
            try
            {
                var visitor = await _visitorRepo.GetByIdAsync(visitorId)
                    ?? throw new AppException("Visitor record not found.", 404);

                var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                    ?? throw new AppException("Resident profile not found.", 404);

                if (visitor.ResidentId != resident.Id)
                    throw new AppException("You are not authorized to approve this visitor.", 403);

                if (visitor.ApprovalStatus != VisitorStatus.Pending)
                    throw new AppException("Visitor request already actioned.", 409);

                visitor.ApprovalStatus = VisitorStatus.Approved;
                visitor.ApprovedBy = callerUserId;
                visitor.ApprovedAt = DateTime.UtcNow;

                await _visitorRepo.UpdateAsync(visitor);
                await _visitorRepo.SaveChangesAsync();

                // TODO: Dispatch FCM to security officer (Phase 1.7)
                _logger.LogInformation("Visitor {VisitorId} approved by {UserId}", visitorId, callerUserId);
                await _notificationService.CreateAndSendAsync(
                    visitor.SecurityId,
                    "Visitor Approved",
                    $"{visitor.VisitorName} has been approved by the resident.",
                    "VisitorApproval",
                    visitor.Id);

                return MapToDetailDto(visitor);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveVisitorAsync failed for {VisitorId}", visitorId);
                throw new AppException("Failed to approve visitor.", 500);
            }
        }



        // ── Reject Visitor ───────────────────────────────────────────
        public async Task<VisitorDetailDto> RejectVisitorAsync(Guid visitorId, ApproveRejectVisitorRequestDto dto, Guid callerUserId)
        {
            try
            {
                var visitor = await _visitorRepo.GetByIdAsync(visitorId)
                    ?? throw new AppException("Visitor record not found.", 404);

                var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                    ?? throw new AppException("Resident profile not found.", 404);

                if (visitor.ResidentId != resident.Id)
                    throw new AppException("You are not authorized to reject this visitor.", 403);

                if (visitor.ApprovalStatus != VisitorStatus.Pending)
                    throw new AppException("Visitor request already actioned.", 409);

                visitor.ApprovalStatus = VisitorStatus.Rejected;
                visitor.ApprovedBy = callerUserId;
                visitor.ApprovedAt = DateTime.UtcNow;

                await _visitorRepo.UpdateAsync(visitor);
                await _visitorRepo.SaveChangesAsync();

                // TODO: Dispatch FCM to security officer (Phase 1.7)
                _logger.LogInformation("Visitor {VisitorId} rejected by {UserId}", visitorId, callerUserId);
                await _notificationService.CreateAndSendAsync(
                    visitor.SecurityId,
                    "Visitor Rejected",
                    $"{visitor.VisitorName} has been rejected by the resident.",
                    "VisitorApproval",
                    visitor.Id);

                return MapToDetailDto(visitor);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RejectVisitorAsync failed for {VisitorId}", visitorId);
                throw new AppException("Failed to reject visitor.", 500);
            }
        }



        // ── Register Exit ────────────────────────────────────────────
        public async Task<VisitorDetailDto> RegisterExitAsync(Guid visitorId)
        {
            try
            {
                var visitor = await _visitorRepo.GetByIdAsync(visitorId)
                    ?? throw new AppException("Visitor record not found.", 404);

                if (visitor.ExitTime != null)
                    throw new AppException("Visitor exit already recorded.", 409);

                visitor.ExitTime = DateTime.UtcNow;

                await _visitorRepo.UpdateAsync(visitor);
                await _visitorRepo.SaveChangesAsync();

                return MapToDetailDto(visitor);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterExitAsync failed for {VisitorId}", visitorId);
                throw new AppException("Failed to register visitor exit.", 500);
            }
        }



        // ── Pending Approvals ─────────────────────────────────────────
        public async Task<List<VisitorListDto>> GetPendingApprovalsAsync(Guid callerUserId)
        {
            try
            {
                var resident = await _residentRepo.GetByUserIdAsync(callerUserId)
                    ?? throw new AppException("Resident profile not found.", 404);

                var visitors = await _visitorRepo.GetPendingByResidentIdAsync(resident.Id);
                return visitors.Select(MapToListDto).ToList();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPendingApprovalsAsync failed for {UserId}", callerUserId);
                throw new AppException("Failed to retrieve pending approvals.", 500);
            }
        }



        // ── Mappers ──────────────────────────────────────────────────
        private static VisitorDetailDto MapToDetailDto(Visitor v) => new()
        {
            Id = v.Id,
            ResidentId = v.ResidentId,
            ResidentName = v.Resident?.User?.FullName ?? string.Empty,
            ApartmentNumber = v.Resident?.ApartmentNumber ?? string.Empty,
            Block = v.Resident?.Block ?? string.Empty,
            SecurityId = v.SecurityId,
            SecurityName = v.Security?.FullName ?? string.Empty,
            VisitorName = v.VisitorName,
            VisitorPhone = v.VisitorPhone,
            Purpose = v.Purpose,
            VehicleNumber = v.VehicleNumber,
            EntryTime = v.EntryTime,
            ExitTime = v.ExitTime,
            ApprovalStatus = v.ApprovalStatus.ToString(),
            ApprovedBy = v.ApprovedBy,
            ApprovedAt = v.ApprovedAt,
            CreatedAt = v.CreatedAt
        };

        private static VisitorListDto MapToListDto(Visitor v) => new()
        {
            Id = v.Id,
            VisitorName = v.VisitorName,
            VisitorPhone = v.VisitorPhone,
            Purpose = v.Purpose,
            ResidentName = v.Resident?.User?.FullName ?? string.Empty,
            ApartmentNumber = v.Resident?.ApartmentNumber ?? string.Empty,
            Block = v.Resident?.Block ?? string.Empty,
            EntryTime = v.EntryTime,
            ExitTime = v.ExitTime,
            ApprovalStatus = v.ApprovalStatus.ToString(),
            CreatedAt = v.CreatedAt
        };
    }
}
