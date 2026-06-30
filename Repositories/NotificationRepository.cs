using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(List<Notification> Notifications, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize)
        {
            var query = _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task<Notification?> GetByIdAsync(Guid notificationId)
        {
            return await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task AddAsync(Notification notification)
        {
            await _db.Notifications.AddAsync(notification);
        }

        public Task MarkAsReadAsync(Notification notification)
        {
            notification.IsRead = true;
            _db.Notifications.Update(notification);
            return Task.CompletedTask;
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            _db.Notifications.UpdateRange(unread);
        }

        public Task DeleteAsync(Notification notification)
        {
            _db.Notifications.Remove(notification);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

}
