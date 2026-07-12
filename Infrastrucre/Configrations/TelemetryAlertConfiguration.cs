using HerdSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class TelemetryAlertConfiguration : IEntityTypeConfiguration<TelemetryAlert>
    {
        public void Configure(EntityTypeBuilder<TelemetryAlert> builder)
        {
            builder.ToTable("TelemetryAlerts");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.SensorType).HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.Value).HasPrecision(18, 2);
            builder.Property(a => a.Message).HasMaxLength(500).IsRequired();
            builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(a => a.CattleId);
            builder.HasIndex(a => a.IsResolved);

            builder.HasOne(a => a.Cattle)
                .WithMany(c => c.Alerts)
                .HasForeignKey(a => a.CattleId)
                .OnDelete(DeleteBehavior.Cascade);

            var ulidConverter = new ValueConverter<Ulid, string>(
      v => v.ToString(),
      v => Ulid.Parse(v));
            builder.Property(c => c.Id).HasConversion(ulidConverter);
            builder.Property(c => c.TenantId).HasConversion(ulidConverter);
        }
    }
}