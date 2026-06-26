using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartApart.API.Entities;

namespace SmartApart.API.Data.Configurations
{
    public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
    {
        public void Configure(EntityTypeBuilder<FamilyMember> builder)
        {
            builder.HasKey(fm => fm.Id);

            builder.Property(fm => fm.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(fm => fm.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(fm => fm.Relationship)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(fm => fm.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(fm => fm.DateOfBirth)
                .IsRequired(false);

            builder.Property(fm => fm.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasIndex(fm => fm.ResidentId);
        }
    }
}
