using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class ResidentConfiguration : IEntityTypeConfiguration<Resident>
    {
        public void Configure(EntityTypeBuilder<Resident> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(r => r.ApartmentNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.Block)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(r => r.EmergencyContactName)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(r => r.EmergencyContactPhone)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(r => r.MoveOutDate)
                .IsRequired(false);

            // Relationships
            builder.HasMany(r => r.FamilyMembers)
                .WithOne(fm => fm.Resident)
                .HasForeignKey(fm => fm.ResidentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Visitors)
                .WithOne(v => v.Resident)
                .HasForeignKey(v => v.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.MaintenanceRequests)
                .WithOne(mr => mr.Resident)
                .HasForeignKey(mr => mr.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.Complaints)
                .WithOne(c => c.Resident)
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(r => r.UserId).IsUnique();
            builder.HasIndex(r => r.ApartmentNumber);
            builder.HasIndex(r => r.Block);
        }
    }
}
