using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Announcements
{
    public class AnnouncementListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public NoticeType NoticeType { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
