namespace SmartApart.API.DTOs.Residents
{
    public class ResidentDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePhotoUrl { get; set; }
        public string ApartmentNumber { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public int Floor { get; set; }
        public string OwnershipType { get; set; } = string.Empty;
        public DateOnly MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
    }
}
