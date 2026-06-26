using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class AnnouncementAttachmentConfiguration : IEntityTypeConfiguration<AnnouncementAttachment>
    {
        public void Configure(EntityTypeBuilder<AnnouncementAttachment> builder)
        {
            builder.HasKey(aa => aa.Id);

            builder.Property(aa => aa.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(aa => aa.FileUrl)
                .IsRequired();

            builder.Property(aa => aa.UploadedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(aa => aa.AnnouncementId);
        }
    }
}
