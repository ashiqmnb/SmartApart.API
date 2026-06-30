namespace SmartApart.API.DTOs.Amenities
{
    public class CreateAmenityRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public string? Rules { get; set; }

    }
}
