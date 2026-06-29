namespace SmartApart.API.DTOs.Users
{
    public class UpdateProfileRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
