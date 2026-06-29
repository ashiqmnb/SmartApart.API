namespace SmartApart.API.DTOs.Residents
{
    public class UpdateFamilyMemberRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
