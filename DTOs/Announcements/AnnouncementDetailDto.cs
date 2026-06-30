using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Announcements
{
    public class AnnouncementDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NoticeType NoticeType { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<AnnouncementAttachmentDto> Attachments { get; set; } = new();

    }
}
