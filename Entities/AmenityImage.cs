namespace SmartApart.API.Entities
{
    public class AmenityImage
    {
        public Guid Id { get; set; }
        public Guid AmenityId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Amenity Amenity { get; set; } = null!;

    }
}
