using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Amenities;

public class UpdateAmenityAvailabilityRequestDto
{
    public AmenityStatus Availability { get; set; }
}