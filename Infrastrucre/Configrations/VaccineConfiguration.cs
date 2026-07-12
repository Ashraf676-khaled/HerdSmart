// HerdSmart.Infrastructure/Configurations/VaccineConfiguration.cs
using HerdSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class VaccineConfiguration : IEntityTypeConfiguration<Vaccine>
    {
        public void Configure(EntityTypeBuilder<Vaccine> builder)
        {
            builder.ToTable("Vaccines");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(v => v.TargetAgeInMonths)
                .IsRequired();

            builder.Property(v => v.Dosage)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(v => v.IntervalInDays)
                .IsRequired(false);

            builder.Property(v => v.CreatedAt)
                .IsRequired();

            // Index
            builder.HasIndex(v => v.Name);

            // العلاقة مع Tenant (اختياري لأن TenantId nullable)
            builder.HasOne(v => v.Tenant)
                .WithMany()  // لو Tenant مش عنده مجموعة Vaccines
                .HasForeignKey(v => v.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // محول Ulid -> string
            var ulidConverter = new ValueConverter<Ulid, string>(
       v => v.ToString(),
       v => Ulid.Parse(v));
            builder.Property(c => c.Id).HasConversion(ulidConverter);
            builder.Property(c => c.TenantId).HasConversion(ulidConverter);

        }
    }
}
