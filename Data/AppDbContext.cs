using Microsoft.EntityFrameworkCore;
using SmartApart.API.Entities;
using SmartApart.API.Enums;

namespace SmartApart.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
        public DbSet<Resident> Residents => Set<Resident>();
        public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
        public DbSet<Visitor> Visitors => Set<Visitor>();
        public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
        public DbSet<MaintenanceImage> MaintenanceImages => Set<MaintenanceImage>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<ComplaintImage> ComplaintImages => Set<ComplaintImage>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<AnnouncementAttachment> AnnouncementAttachments => Set<AnnouncementAttachment>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<AmenityImage> AmenityImages => Set<AmenityImage>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<FcmToken> FcmTokens => Set<FcmToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Register all enum conversions to store as strings in PostgreSQL
            modelBuilder.HasPostgresEnum<Role>();
            modelBuilder.HasPostgresEnum<OwnershipType>();
            modelBuilder.HasPostgresEnum<VisitorStatus>();
            modelBuilder.HasPostgresEnum<MaintenanceCategory>();
            modelBuilder.HasPostgresEnum<Priority>();
            modelBuilder.HasPostgresEnum<MaintenanceStatus>();
            modelBuilder.HasPostgresEnum<ComplaintCategory>();
            modelBuilder.HasPostgresEnum<ComplaintType>();
            modelBuilder.HasPostgresEnum<ComplaintStatus>();
            modelBuilder.HasPostgresEnum<NoticeType>();
            modelBuilder.HasPostgresEnum<AmenityStatus>();
            modelBuilder.HasPostgresEnum<NotificationType>();
            modelBuilder.HasPostgresEnum<OtpPurpose>();
            modelBuilder.HasPostgresEnum<AttachmentFileType>();
            modelBuilder.HasPostgresEnum<DeviceType>();

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
