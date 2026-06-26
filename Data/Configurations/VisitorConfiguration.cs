using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class VisitorConfiguration : IEntityTypeConfiguration<Visitor>
    {
        public void Configure(EntityTypeBuilder<Visitor> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(v => v.VisitorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.VisitorPhone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.Purpose)
                .IsRequired();

            builder.Property(v => v.VehicleNumber)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(v => v.ExitTime)
                .IsRequired(false);

            builder.Property(v => v.ApprovedBy)
                .IsRequired(false);

            builder.Property(v => v.ApprovedAt)
                .IsRequired(false);

            builder.Property(v => v.CreatedAt)
                .HasDefaultValueSql("now()");

            // Relationships
            builder.HasOne(v => v.Security)
                .WithMany()
                .HasForeignKey(v => v.SecurityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.Approver)
                .WithMany()
                .HasForeignKey(v => v.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(v => v.ResidentId);
            builder.HasIndex(v => v.ApprovalStatus);
        }
    }
}
