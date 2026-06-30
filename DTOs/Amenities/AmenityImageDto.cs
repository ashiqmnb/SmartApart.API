namespace SmartApart.API.DTOs.Amenities;

public class AmenityImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}