using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class AmenityImageConfiguration : IEntityTypeConfiguration<AmenityImage>
    {
        public void Configure(EntityTypeBuilder<AmenityImage> builder)
        {
            builder.HasKey(ai => ai.Id);

            builder.Property(ai => ai.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(ai => ai.ImageUrl)
                .IsRequired();

            builder.Property(ai => ai.UploadedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(ai => ai.AmenityId);
        }
    }
}
