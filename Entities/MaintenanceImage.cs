namespace SmartApart.API.Entities
{
    public class MaintenanceImage
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public MaintenanceRequest MaintenanceRequest { get; set; } = null!;
    }
}
