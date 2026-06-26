using FirebaseAdmin.Messaging;
using SmartApart.API.Enums;

namespace SmartApart.API.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Resident? Resident { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<FcmToken> FcmTokens { get; set; } = new List<FcmToken>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}
