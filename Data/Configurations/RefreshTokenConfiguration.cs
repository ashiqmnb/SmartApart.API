using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(rt => rt.Token)
                .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                .HasDefaultValue(false);

            builder.Property(rt => rt.CreatedAt)
                .HasDefaultValueSql("now()");

            // Index for fast token lookup
            builder.HasIndex(rt => rt.Token);
            builder.HasIndex(rt => rt.UserId);
        }
    }
}
