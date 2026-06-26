using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(n => n.Body)
                .IsRequired();

            builder.Property(n => n.ReferenceId)
                .IsRequired(false);

            builder.Property(n => n.ReferenceType)
                .IsRequired(false);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("now()");

            // Indexes
            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.IsRead);
        }
    }
}
