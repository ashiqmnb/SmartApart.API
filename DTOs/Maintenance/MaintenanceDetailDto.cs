namespace SmartApart.API.DTOs.Maintenance
{
    public class MaintenanceDetailDto
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? AssignedTo { get; set; }
        public string? AssignedStaffName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
