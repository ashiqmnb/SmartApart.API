namespace SmartApart.API.DTOs.Residents
{
    public class UpdateResidentRequestDto
    {
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
