namespace SmartApart.API.DTOs.Maintenance
{
    public class UpdateMaintenanceStatusRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public string? ResolutionNote { get; set; }
    }
}
