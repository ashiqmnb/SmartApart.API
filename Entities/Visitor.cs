using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class Visitor
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public Guid SecurityId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string VisitorPhone { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public VisitorStatus ApprovalStatus { get; set; } = VisitorStatus.Pending;
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Resident Resident { get; set; } = null!;
        public User Security { get; set; } = null!;
        public User? Approver { get; set; }
    }
}
