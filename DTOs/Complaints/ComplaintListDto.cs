namespace SmartApart.API.DTOs.Complaints
{
    public class ComplaintListDto
    {
        public Guid Id { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ComplaintType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
