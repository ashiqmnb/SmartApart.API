namespace SmartApart.API.Entities
{
    public class ComplaintImage
    {
        public Guid Id { get; set; }
        public Guid ComplaintId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Complaint Complaint { get; set; } = null!;
    }
}
