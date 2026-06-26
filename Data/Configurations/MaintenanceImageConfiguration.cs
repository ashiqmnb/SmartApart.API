using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class MaintenanceImageConfiguration : IEntityTypeConfiguration<MaintenanceImage>
    {
        public void Configure(EntityTypeBuilder<MaintenanceImage> builder)
        {
            builder.HasKey(mi => mi.Id);

            builder.Property(mi => mi.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(mi => mi.ImageUrl)
                .IsRequired();

            builder.Property(mi => mi.UploadedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(mi => mi.RequestId);
        }
    }
}
