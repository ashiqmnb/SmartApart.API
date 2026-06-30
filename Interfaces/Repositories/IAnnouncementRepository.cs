using SmartApart.API.Entities;
using SmartApart.API.Enums;

namespace SmartApart.API.Interfaces.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<Announcement?> GetByIdAsync(Guid announcementId);
        Task<(List<Announcement> Announcements, int TotalCount)> GetPublishedAsync(
            NoticeType? noticeType, int page, int pageSize);
        Task<(List<Announcement> Announcements, int TotalCount)> GetDraftsAsync(int page, int pageSize);
        Task<List<Announcement>> GetDueForPublishingAsync();

        Task AddAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(Announcement announcement);

        Task AddAttachmentAsync(AnnouncementAttachment attachment);
        Task<AnnouncementAttachment?> GetAttachmentByIdAsync(Guid attachmentId);
        Task DeleteAttachmentAsync(AnnouncementAttachment attachment);

        Task SaveChangesAsync();

    }
}
