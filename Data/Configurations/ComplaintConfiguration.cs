using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
    {
        public void Configure(EntityTypeBuilder<Complaint> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Description)
                .IsRequired();

            builder.Property(c => c.ResolutionNote)
                .IsRequired(false);

            builder.Property(c => c.ResolvedAt)
                .IsRequired(false);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("now()");

            // Relationships
            builder.HasMany(c => c.Images)
                .WithOne(i => i.Complaint)
                .HasForeignKey(i => i.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => c.ResidentId);
            builder.HasIndex(c => c.Status);
        }
    }
}
