using SmartApart.API.Enums;

namespace SmartApart.API.DTOs.Complaints
{
    public class CreateComplaintRequestDto
    {
        public ComplaintCategory Category { get; set; }
        public ComplaintType ComplaintType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

    }
}
