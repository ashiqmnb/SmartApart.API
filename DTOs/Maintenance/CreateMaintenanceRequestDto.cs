using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Maintenance
{
    public class CreateMaintenanceRequestDto
    {
        public MaintenanceCategory Category { get; set; }
        public Priority Priority { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

    }
}
