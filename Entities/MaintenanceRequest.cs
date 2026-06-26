using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class MaintenanceRequest
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public MaintenanceCategory Category { get; set; }
        public Priority Priority { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Open;
        public Guid? AssignedTo { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Resident Resident { get; set; } = null!;
        public User? AssignedStaff { get; set; }
        public ICollection<MaintenanceImage> Images { get; set; } = new List<MaintenanceImage>();
    }
}
