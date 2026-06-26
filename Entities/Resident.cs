using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class Resident
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ApartmentNumber { get; set; } = string.Empty;
        public string Block { get; set; } = string.Empty;
        public int Floor { get; set; }
        public OwnershipType OwnershipType { get; set; }
        public DateOnly MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
