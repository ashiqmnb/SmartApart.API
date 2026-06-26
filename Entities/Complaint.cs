using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class Complaint
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public ComplaintCategory Category { get; set; }
        public ComplaintType ComplaintType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public string? ResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Resident Resident { get; set; } = null!;
        public ICollection<ComplaintImage> Images { get; set; } = new List<ComplaintImage>();
    }
}
