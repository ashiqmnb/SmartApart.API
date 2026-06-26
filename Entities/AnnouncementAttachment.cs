using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class AnnouncementAttachment
    {
        public Guid Id { get; set; }
        public Guid AnnouncementId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public AttachmentFileType FileType { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Announcement Announcement { get; set; } = null!;

    }
}
