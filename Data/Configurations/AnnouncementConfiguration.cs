using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Body)
                .IsRequired();

            builder.Property(a => a.IsPublished)
                .HasDefaultValue(false);

            builder.Property(a => a.ScheduledAt)
                .IsRequired(false);

            builder.Property(a => a.PublishedAt)
                .IsRequired(false);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("now()");

            // Relationships
            builder.HasMany(a => a.Attachments)
                .WithOne(at => at.Announcement)
                .HasForeignKey(at => at.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(a => a.IsPublished);
            builder.HasIndex(a => a.ScheduledAt);
            builder.HasIndex(a => a.CreatedBy);
        }
    }
}
