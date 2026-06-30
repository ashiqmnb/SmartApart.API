namespace SmartApart.API.DTOs.Complaints
{
    public class ComplaintImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

    }
}
