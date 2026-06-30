namespace SmartApart.API.DTOs.Complaints
{
    public class ComplaintDetailDto
    {
        public Guid Id { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string? ApartmentNumber { get; set; }
        public string? Block { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ComplaintType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
