namespace SmartApart.API.DTOs.Visitors
{
    public class VisitorListDto
    {
        public Guid Id { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string VisitorPhone { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string ResidentName { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
