using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Announcements
{
    public class AnnouncementFilterRequestDto
    {
        public NoticeType? NoticeType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
