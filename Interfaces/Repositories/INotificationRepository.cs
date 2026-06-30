using SmartApart.API.Entities;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<(List<Notification> Notifications, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize);
        Task<Notification?> GetByIdAsync(Guid notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(Notification notification);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteAsync(Notification notification);
        Task SaveChangesAsync();

    }
}
