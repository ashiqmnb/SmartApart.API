namespace SmartApart.API.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }
}
