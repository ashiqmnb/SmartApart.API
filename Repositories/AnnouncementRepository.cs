using Microsoft.EntityFrameworkCore;
using SmartApart.API.Data;
using SmartApart.API.Entities;
using SmartApart.API.Enums;
using SmartApart.API.Interfaces.Repositories;

namespace SmartApart.API.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly AppDbContext _db;

        public AnnouncementRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Announcement?> GetByIdAsync(Guid announcementId)
        {
            return await _db.Announcements
                .Include(a => a.Creator)
                .Include(a => a.Attachments)
                .FirstOrDefaultAsync(a => a.Id == announcementId);
        }

        public async Task<(List<Announcement> Announcements, int TotalCount)> GetPublishedAsync(
            NoticeType? noticeType, int page, int pageSize)
        {
            var query = _db.Announcements
                .Include(a => a.Creator)
                .Include(a => a.Attachments)
                .Where(a => a.IsPublished)
                .AsQueryable();

            if (noticeType.HasValue)
                query = query.Where(a => a.NoticeType == noticeType.Value);

            var totalCount = await query.CountAsync();

            var announcements = await query
                .OrderByDescending(a => a.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (announcements, totalCount);
        }

        public async Task<(List<Announcement> Announcements, int TotalCount)> GetDraftsAsync(int page, int pageSize)
        {
            var query = _db.Announcements
                .Include(a => a.Creator)
                .Where(a => !a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var announcements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (announcements, totalCount);
        }

        public async Task<List<Announcement>> GetDueForPublishingAsync()
        {
            var now = DateTime.UtcNow;
            return await _db.Announcements
                .Where(a => !a.IsPublished && a.ScheduledAt != null && a.ScheduledAt <= now)
                .ToListAsync();
        }

        public async Task AddAsync(Announcement announcement)
        {
            await _db.Announcements.AddAsync(announcement);
        }

        public Task UpdateAsync(Announcement announcement)
        {
            _db.Announcements.Update(announcement);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Announcement announcement)
        {
            _db.Announcements.Remove(announcement);
            return Task.CompletedTask;
        }

        public async Task AddAttachmentAsync(AnnouncementAttachment attachment)
        {
            await _db.AnnouncementAttachments.AddAsync(attachment);
        }

        public async Task<AnnouncementAttachment?> GetAttachmentByIdAsync(Guid attachmentId)
        {
            return await _db.AnnouncementAttachments
                .FirstOrDefaultAsync(at => at.Id == attachmentId);
        }

        public Task DeleteAttachmentAsync(AnnouncementAttachment attachment)
        {
            _db.AnnouncementAttachments.Remove(attachment);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }

}
