namespace SmartApart.API.DTOs.Complaints
{
    public class ComplaintFilterRequestDto
    {
        public string? Status { get; set; }
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
