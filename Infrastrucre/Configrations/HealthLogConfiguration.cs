// HerdSmart.Infrastructure/Configurations/HealthLogConfiguration.cs
using HerdSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class HealthLogConfiguration : IEntityTypeConfiguration<HealthLog>
    {
        public void Configure(EntityTypeBuilder<HealthLog> builder)
        {
            builder.ToTable("HealthLogs");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Diagnosis)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(h => h.TreatmentPlan)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(h => h.VetNotes)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasIndex(h => h.CattleId);

            builder.HasOne(h => h.Cattle)
                .WithMany(c => c.HealthLogs)
                .HasForeignKey(h => h.CattleId)
                .OnDelete(DeleteBehavior.Cascade);

            var ulidConverter = new ValueConverter<Ulid, string>(
      v => v.ToString(),
      v => Ulid.Parse(v));
            builder.Property(c => c.Id).HasConversion(ulidConverter);
            builder.Property(c => c.TenantId).HasConversion(ulidConverter);
        }
    }
}