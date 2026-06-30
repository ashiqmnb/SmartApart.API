using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Amenities;

public class AmenityDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public string? Rules { get; set; }
    public AmenityStatus Availability { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AmenityImageDto> Images { get; set; } = new();
}