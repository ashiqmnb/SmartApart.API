namespace SmartApart.API.DTOs.Maintenance
{
    public class MaintenanceFilterRequestDto
    {
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
