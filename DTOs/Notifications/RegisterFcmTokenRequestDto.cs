namespace SmartApart.API.DTOs.Notifications
{
    public class RegisterFcmTokenRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // "Android" or "iOS"

    }
}
