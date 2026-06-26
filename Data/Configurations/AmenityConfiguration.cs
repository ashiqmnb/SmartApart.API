using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
    {
        public void Configure(EntityTypeBuilder<Amenity> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Description)
                .IsRequired();

            builder.Property(a => a.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Rules)
                .IsRequired(false);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("now()");

            // Relationships
            builder.HasMany(a => a.Images)
                .WithOne(i => i.Amenity)
                .HasForeignKey(i => i.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.Availability);
        }
    }
}
