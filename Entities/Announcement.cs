using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NoticeType NoticeType { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User Creator { get; set; } = null!;
        public ICollection<AnnouncementAttachment> Attachments { get; set; } = new List<AnnouncementAttachment>();

    }
}
