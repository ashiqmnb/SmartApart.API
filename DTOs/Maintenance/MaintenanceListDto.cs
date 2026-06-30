namespace SmartApart.API.DTOs.Maintenance
{
    public class MaintenanceListDto
    {
        public Guid Id { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
