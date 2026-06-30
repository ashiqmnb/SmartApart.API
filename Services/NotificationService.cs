using SmartApart.API.Common;
using SmartApart.API.DTOs.Notifications;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IFcmTokenRepository _fcmTokenRepo;
        private readonly IFcmService _fcmService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepo,
            IFcmTokenRepository fcmTokenRepo,
            IFcmService fcmService,
            ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _fcmTokenRepo = fcmTokenRepo;
            _fcmService = fcmService;
            _logger = logger;
        }

        // ── Get Notifications (Paginated Feed) ──────────────────────────

        public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page, int pageSize)
        {
            try
            {
                var (notifications, totalCount) = await _notificationRepo.GetByUserIdAsync(userId, page, pageSize);

                return new PagedResult<NotificationDto>
                {
                    Items = notifications.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetNotificationsAsync failed for {UserId}", userId);
                throw new AppException("Failed to retrieve notifications.", 500);
            }
        }

        // ── Unread Count ─────────────────────────────────────────────────

        public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _notificationRepo.GetUnreadCountAsync(userId);
                return new UnreadCountDto { UnreadCount = count };
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUnreadCountAsync failed for {UserId}", userId);
                throw new AppException("Failed to retrieve unread count.", 500);
            }
        }

        // ── Mark As Read ─────────────────────────────────────────────────

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var notification = await _notificationRepo.GetByIdAsync(notificationId)
                    ?? throw new AppException("Notification not found.", 404);

                if (notification.UserId != userId)
                    throw new AppException("You are not authorized to modify this notification.", 403);

                await _notificationRepo.MarkAsReadAsync(notification);
                await _notificationRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkAsReadAsync failed for {NotificationId}", notificationId);
                throw new AppException("Failed to mark notification as read.", 500);
            }
        }

        // ── Mark All As Read ─────────────────────────────────────────────

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                await _notificationRepo.MarkAllAsReadAsync(userId);
                await _notificationRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkAllAsReadAsync failed for {UserId}", userId);
                throw new AppException("Failed to mark all notifications as read.", 500);
            }
        }

        // ── Delete Notification (soft — user's own only) ────────────────

        public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var notification = await _notificationRepo.GetByIdAsync(notificationId)
                    ?? throw new AppException("Notification not found.", 404);

                if (notification.UserId != userId)
                    throw new AppException("You are not authorized to delete this notification.", 403);

                await _notificationRepo.DeleteAsync(notification);
                await _notificationRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteNotificationAsync failed for {NotificationId}", notificationId);
                throw new AppException("Failed to delete notification.", 500);
            }
        }

        // ── Create + Send (called internally by other services) ─────────

        public async Task CreateAndSendAsync(Guid userId, string title, string body, string type, Guid? referenceId)
        {
            try
            {
                if (!Enum.TryParse<NotificationType>(type, out var notificationType))
                {
                    _logger.LogWarning("Invalid notification type {Type} — skipping persist, attempting send only.", type);
                }

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Body = body,
                    Type = notificationType,
                    ReferenceId = referenceId,
                    ReferenceType = type,
                    IsRead = false,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepo.AddAsync(notification);
                await _notificationRepo.SaveChangesAsync();

                var data = new Dictionary<string, string>
            {
                { "type", type },
                { "referenceId", referenceId?.ToString() ?? string.Empty }
            };

                await _fcmService.SendToUserAsync(userId, title, body, data);
            }
            catch (Exception ex)
            {
                // Notification dispatch failures should never break the calling operation
                _logger.LogError(ex, "CreateAndSendAsync failed for user {UserId}", userId);
            }
        }

        // ── Register FCM Token ────────────────────────────────────────────

        public async Task RegisterFcmTokenAsync(Guid userId, RegisterFcmTokenRequestDto dto)
        {
            try
            {
                if (!Enum.TryParse<DeviceType>(dto.DeviceType, out var deviceType))
                    throw new AppException("Invalid device type.", 400);

                var existing = await _fcmTokenRepo.GetByTokenAsync(dto.Token);

                if (existing != null)
                {
                    existing.UserId = userId;
                    existing.DeviceType = deviceType;
                    existing.IsActive = true;
                    // Reuse AddAsync's underlying SaveChanges via repo update path
                    await _fcmTokenRepo.SaveChangesAsync();
                    return;
                }

                var token = new FcmToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = dto.Token,
                    DeviceType = deviceType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _fcmTokenRepo.AddAsync(token);
                await _fcmTokenRepo.SaveChangesAsync();
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterFcmTokenAsync failed for {UserId}", userId);
                throw new AppException("Failed to register device token.", 500);
            }
        }

        // ── Deregister FCM Token (on logout) ──────────────────────────────

        public async Task DeregisterFcmTokenAsync(Guid userId, DeregisterFcmTokenRequestDto dto)
        {
            try
            {
                var token = await _fcmTokenRepo.GetByTokenAsync(dto.Token);

                if (token == null || token.UserId != userId)
                    return; // Nothing to do — already gone or not owned by this user

                await _fcmTokenRepo.DeactivateAsync(token);
                await _fcmTokenRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeregisterFcmTokenAsync failed for {UserId}", userId);
                // Don't throw — logout should still succeed even if this fails
            }
        }

        // ── Mapper ─────────────────────────────────────────────────────

        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type.ToString(),
            ReferenceId = n.ReferenceId,
            ReferenceType = n.ReferenceType,
            IsRead = n.IsRead,
            SentAt = n.SentAt
        };
    }


}
