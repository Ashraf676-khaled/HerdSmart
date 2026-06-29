// HerdSmart.Infrastructure/Configurations/MilkProductionLogConfiguration.cs
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class MilkProductionLogConfiguration : IEntityTypeConfiguration<MilkProductionLog>
    {
        public void Configure(EntityTypeBuilder<MilkProductionLog> builder)
        {
            builder.ToTable("MilkProductionLogs");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.AmountInLiters)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(m => m.Shift)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(m => m.LoggedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(m => m.CattleId);
            builder.HasIndex(m => m.LoggedAt);
            builder.HasIndex(m => new { m.TenantId, m.LoggedAt });

            // Relationships
            builder.HasOne(m => m.Cattle)
                .WithMany(c => c.MilkLogs)
                .HasForeignKey(m => m.CattleId)
                .OnDelete(DeleteBehavior.Cascade);

            var ulidConverter = new ValueConverter<Ulid, string>(
      v => v.ToString(),
      v => Ulid.Parse(v));
            builder.Property(c => c.Id).HasConversion(ulidConverter);
            builder.Property(c => c.TenantId).HasConversion(ulidConverter);
        }
    }
}