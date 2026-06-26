using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class ComplaintImageConfiguration : IEntityTypeConfiguration<ComplaintImage>
    {
        public void Configure(EntityTypeBuilder<ComplaintImage> builder)
        {
            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(ci => ci.ImageUrl)
                .IsRequired();

            builder.Property(ci => ci.UploadedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(ci => ci.ComplaintId);
        }
    }
}
