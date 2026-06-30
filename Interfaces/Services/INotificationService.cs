using SmartApart.API.Common;
using SmartApart.API.DTOs.Notifications;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Services
{
    public interface INotificationService
    {
        // Notification feed
        Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int page, int pageSize);
        Task<UnreadCountDto> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(Guid notificationId, Guid userId);

        // Internal — used by other services to dispatch + persist notifications
        Task CreateAndSendAsync(Guid userId, string title, string body, string type, Guid? referenceId);

        // FCM token lifecycle
        Task RegisterFcmTokenAsync(Guid userId, RegisterFcmTokenRequestDto dto);
        Task DeregisterFcmTokenAsync(Guid userId, DeregisterFcmTokenRequestDto dto);
    }

}
