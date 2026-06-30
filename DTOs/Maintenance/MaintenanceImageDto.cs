namespace SmartApart.API.DTOs.Maintenance
{
    public class MaintenanceImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

    }
}
