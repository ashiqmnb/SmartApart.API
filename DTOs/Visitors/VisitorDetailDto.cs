namespace SmartApart.API.DTOs.Visitors
{
    public class VisitorDetailDto
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public Guid SecurityId { get; set; }
        public string SecurityName { get; set; } = string.Empty;
        public string VisitorName { get; set; } = string.Empty;
        public string VisitorPhone { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
