namespace SmartApart.API.DTOs.Visitors
{
    public class RegisterVisitorRequestDto
    {
        public Guid ResidentId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string VisitorPhone { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string? VehicleNumber { get; set; }

    }
}
