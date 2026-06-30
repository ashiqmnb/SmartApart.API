using SmartApart.API.Common;
using SmartApart.API.DTOs.Announcements;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepo;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<AnnouncementService> _logger;

        public AnnouncementService(
            IAnnouncementRepository announcementRepo,
            ICloudinaryService cloudinaryService,
            ILogger<AnnouncementService> logger)
        {
            _announcementRepo = announcementRepo;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // ── Create ───────────────────────────────────────────────────

        public async Task<AnnouncementDetailDto> CreateAnnouncementAsync(CreateAnnouncementRequestDto dto, Guid callerUserId)
        {
            try
            {
                var isImmediate = dto.ScheduledAt == null;

                var announcement = new Announcement
                {
                    Id = Guid.NewGuid(),
                    CreatedBy = callerUserId,
                    Title = dto.Title,
                    Body = dto.Body,
                    NoticeType = dto.NoticeType,
                    IsPublished = isImmediate,
                    ScheduledAt = dto.ScheduledAt,
                    PublishedAt = isImmediate ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _announcementRepo.AddAsync(announcement);
                await _announcementRepo.SaveChangesAsync();

                if (isImmediate)
                {
                    // TODO: FCM broadcast to all users (Phase 1.7)
                    _logger.LogInformation("Announcement {Id} published immediately.", announcement.Id);
                }
                else
                {
                    _logger.LogInformation("Announcement {Id} scheduled for {ScheduledAt}.", announcement.Id, dto.ScheduledAt);
                }

                var saved = await _announcementRepo.GetByIdAsync(announcement.Id)
                    ?? throw new AppException("Failed to load created announcement.", 500);

                return MapToDetailDto(saved);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAnnouncementAsync failed for user {UserId}", callerUserId);
                throw new AppException("Failed to create announcement.", 500);
            }
        }

        // ── Get Published (Paginated + Filter) ──────────────────────

        public async Task<PagedResult<AnnouncementListDto>> GetPublishedAsync(AnnouncementFilterRequestDto filter)
        {
            try
            {
                var (announcements, totalCount) = await _announcementRepo.GetPublishedAsync(
                    filter.NoticeType, filter.Page, filter.PageSize);

                return new PagedResult<AnnouncementListDto>
                {
                    Items = announcements.Select(MapToListDto).ToList(),
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPublishedAsync failed");
                throw new AppException("Failed to retrieve announcements.", 500);
            }
        }

        // ── Get Drafts (Admin) ───────────────────────────────────────

        public async Task<PagedResult<AnnouncementListDto>> GetDraftsAsync(int page, int pageSize)
        {
            try
            {
                var (announcements, totalCount) = await _announcementRepo.GetDraftsAsync(page, pageSize);

                return new PagedResult<AnnouncementListDto>
                {
                    Items = announcements.Select(MapToListDto).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDraftsAsync failed");
                throw new AppException("Failed to retrieve drafts.", 500);
            }
        }

        // ── Get By Id ─────────────────────────────────────────────────

        public async Task<AnnouncementDetailDto> GetByIdAsync(Guid announcementId)
        {
            try
            {
                var announcement = await _announcementRepo.GetByIdAsync(announcementId)
                    ?? throw new AppException("Announcement not found.", 404);

                return MapToDetailDto(announcement);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed for {Id}", announcementId);
                throw new AppException("Failed to retrieve announcement.", 500);
            }
        }

        // ── Update (Unpublished Only) ────────────────────────────────

        public async Task<AnnouncementDetailDto> UpdateAnnouncementAsync(Guid announcementId, UpdateAnnouncementRequestDto dto)
        {
            try
            {
                var announcement = await _announcementRepo.GetByIdAsync(announcementId)
                    ?? throw new AppException("Announcement not found.", 404);

                if (announcement.IsPublished)
                    throw new AppException("Published announcements cannot be edited.", 422);

                announcement.Title = dto.Title;
                announcement.Body = dto.Body;
                announcement.NoticeType = dto.NoticeType;
                announcement.ScheduledAt = dto.ScheduledAt;
                announcement.UpdatedAt = DateTime.UtcNow;

                await _announcementRepo.UpdateAsync(announcement);
                await _announcementRepo.SaveChangesAsync();

                return MapToDetailDto(announcement);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAnnouncementAsync failed for {Id}", announcementId);
                throw new AppException("Failed to update announcement.", 500);
            }
        }

        // ── Delete ────────────────────────────────────────────────────

        public async Task DeleteAnnouncementAsync(Guid announcementId)
        {
            try
            {
                var announcement = await _announcementRepo.GetByIdAsync(announcementId)
                    ?? throw new AppException("Announcement not found.", 404);

                foreach (var attachment in announcement.Attachments)
                {
                    await _cloudinaryService.DeleteImageAsync(attachment.FileUrl);
                }

                await _announcementRepo.DeleteAsync(announcement);
                await _announcementRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAnnouncementAsync failed for {Id}", announcementId);
                throw new AppException("Failed to delete announcement.", 500);
            }
        }

        // ── Add Attachment ────────────────────────────────────────────

        public async Task<AnnouncementAttachmentDto> AddAttachmentAsync(Guid announcementId, string fileUrl, string fileType)
        {
            try
            {
                var announcement = await _announcementRepo.GetByIdAsync(announcementId)
                    ?? throw new AppException("Announcement not found.", 404);

                if (!Enum.TryParse<AttachmentFileType>(fileType, true, out var parsedType))
                    throw new AppException("Invalid attachment file type.", 400);

                var attachment = new AnnouncementAttachment
                {
                    Id = Guid.NewGuid(),
                    AnnouncementId = announcementId,
                    FileUrl = fileUrl,
                    FileType = parsedType,
                    UploadedAt = DateTime.UtcNow
                };

                await _announcementRepo.AddAttachmentAsync(attachment);
                await _announcementRepo.SaveChangesAsync();

                return new AnnouncementAttachmentDto
                {
                    Id = attachment.Id,
                    FileUrl = attachment.FileUrl,
                    FileType = attachment.FileType,
                    UploadedAt = attachment.UploadedAt
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddAttachmentAsync failed for {Id}", announcementId);
                throw new AppException("Failed to upload attachment.", 500);
            }
        }

        // ── Delete Attachment ─────────────────────────────────────────

        public async Task DeleteAttachmentAsync(Guid announcementId, Guid attachmentId)
        {
            try
            {
                var attachment = await _announcementRepo.GetAttachmentByIdAsync(attachmentId)
                    ?? throw new AppException("Attachment not found.", 404);

                if (attachment.AnnouncementId != announcementId)
                    throw new AppException("Attachment does not belong to this announcement.", 400);

                await _cloudinaryService.DeleteImageAsync(attachment.FileUrl);

                await _announcementRepo.DeleteAttachmentAsync(attachment);
                await _announcementRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAttachmentAsync failed for {AttachmentId}", attachmentId);
                throw new AppException("Failed to delete attachment.", 500);
            }
        }

        // ── Manual Publish (Scheduled Draft → Now) ───────────────────

        public async Task<AnnouncementDetailDto> PublishNowAsync(Guid announcementId)
        {
            try
            {
                var announcement = await _announcementRepo.GetByIdAsync(announcementId)
                    ?? throw new AppException("Announcement not found.", 404);

                if (announcement.IsPublished)
                    throw new AppException("Announcement is already published.", 422);

                announcement.IsPublished = true;
                announcement.PublishedAt = DateTime.UtcNow;
                announcement.UpdatedAt = DateTime.UtcNow;

                await _announcementRepo.UpdateAsync(announcement);
                await _announcementRepo.SaveChangesAsync();

                // TODO: FCM broadcast to all users (Phase 1.7)
                _logger.LogInformation("Announcement {Id} manually published.", announcementId);

                return MapToDetailDto(announcement);
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishNowAsync failed for {Id}", announcementId);
                throw new AppException("Failed to publish announcement.", 500);
            }
        }

        // ── Mappers ──────────────────────────────────────────────────

        private static AnnouncementDetailDto MapToDetailDto(Announcement a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Body = a.Body,
            NoticeType = a.NoticeType,
            IsPublished = a.IsPublished,
            ScheduledAt = a.ScheduledAt,
            PublishedAt = a.PublishedAt,
            CreatedByName = a.Creator?.FullName ?? string.Empty,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            Attachments = a.Attachments?.Select(at => new AnnouncementAttachmentDto
            {
                Id = at.Id,
                FileUrl = at.FileUrl,
                FileType = at.FileType,
                UploadedAt = at.UploadedAt
            }).ToList() ?? new()
        };

        private static AnnouncementListDto MapToListDto(Announcement a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            NoticeType = a.NoticeType,
            IsPublished = a.IsPublished,
            ScheduledAt = a.ScheduledAt,
            PublishedAt = a.PublishedAt,
            CreatedAt = a.CreatedAt
        };
    }
}
