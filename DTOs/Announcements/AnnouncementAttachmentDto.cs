using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Announcements
{
    public class AnnouncementAttachmentDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public AttachmentFileType FileType { get; set; }
        public DateTime UploadedAt { get; set; }

    }
}
