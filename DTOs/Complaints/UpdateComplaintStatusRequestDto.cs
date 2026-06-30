namespace SmartApart.API.DTOs.Complaints
{
    public class UpdateComplaintStatusRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public string? ResolutionNote { get; set; }

    }
}
