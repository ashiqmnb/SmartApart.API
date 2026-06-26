using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class Amenity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public string? Rules { get; set; }
        public AmenityStatus Availability { get; set; } = AmenityStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<AmenityImage> Images { get; set; } = new List<AmenityImage>();
    }
}
