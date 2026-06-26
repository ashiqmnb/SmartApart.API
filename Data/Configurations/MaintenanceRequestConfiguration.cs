using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
    {
        public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
        {
            builder.HasKey(mr => mr.Id);

            builder.Property(mr => mr.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(mr => mr.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(mr => mr.Description)
                .IsRequired();

            builder.Property(mr => mr.AssignedTo)
                .IsRequired(false);

            builder.Property(mr => mr.AssignedAt)
                .IsRequired(false);

            builder.Property(mr => mr.ResolvedAt)
                .IsRequired(false);

            builder.Property(mr => mr.ResolutionNote)
                .IsRequired(false);

            builder.Property(mr => mr.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.Property(mr => mr.UpdatedAt)
                .HasDefaultValueSql("now()");

            // Relationships
            builder.HasOne(mr => mr.AssignedStaff)
                .WithMany()
                .HasForeignKey(mr => mr.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(mr => mr.Images)
                .WithOne(i => i.MaintenanceRequest)
                .HasForeignKey(i => i.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(mr => mr.ResidentId);
            builder.HasIndex(mr => mr.Status);
            builder.HasIndex(mr => mr.Priority);
        }
    }
}
