using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Announcements
{
    public class UpdateAnnouncementRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NoticeType NoticeType { get; set; }
        public DateTime? ScheduledAt { get; set; }

    }
}
