using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
    {
        public void Configure(EntityTypeBuilder<OtpVerification> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(o => o.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.OtpCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(o => o.IsUsed)
                .HasDefaultValue(false);

            builder.Property(o => o.CreatedAt)
                .HasDefaultValueSql("now()");

            // Index for fast OTP lookup by phone
            builder.HasIndex(o => o.PhoneNumber);
        }
    }
}
