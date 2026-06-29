namespace SmartApart.API.DTOs.Residents
{
    public class ResidentPublicDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public string ApartmentNumber { get; set; } = string.Empty;
        public int Floor { get; set; }
        public string? ProfilePhotoUrl { get; set; }
    }
}
