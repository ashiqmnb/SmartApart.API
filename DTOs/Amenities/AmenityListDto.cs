using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Amenities;

public class AmenityListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public AmenityStatus Availability { get; set; }
    public string? ThumbnailUrl { get; set; }
}