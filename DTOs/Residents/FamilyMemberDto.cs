namespace SmartApart.API.DTOs.Residents
{
    public class FamilyMemberDto
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
