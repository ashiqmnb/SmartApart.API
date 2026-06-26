using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class FcmTokenConfiguration : IEntityTypeConfiguration<FcmToken>
    {
        public void Configure(EntityTypeBuilder<FcmToken> builder)
        {
            builder.HasKey(ft => ft.Id);

            builder.Property(ft => ft.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(ft => ft.Token)
                .IsRequired();

            builder.Property(ft => ft.IsActive)
                .HasDefaultValue(true);

            builder.Property(ft => ft.CreatedAt)
                .HasDefaultValueSql("now()");

            // Indexes
            builder.HasIndex(ft => ft.UserId);
            builder.HasIndex(ft => ft.Token);
            builder.HasIndex(ft => ft.IsActive);
        }
    }
}
