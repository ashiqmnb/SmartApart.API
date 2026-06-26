namespace SmartApart.API.DTOs.Auth
{
    public class ResendOtpRequestDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }
}
