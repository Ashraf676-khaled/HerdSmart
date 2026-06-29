using HerdSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class CattleConfiguration : IEntityTypeConfiguration<Cattle>
    {
        public void Configure(EntityTypeBuilder<Cattle> builder)
        {
            var ulidConverter = new ValueConverter<Ulid, string>(
       v => v.ToString(),
       v => Ulid.Parse(v));
            builder.Property(c => c.Id).HasConversion(ulidConverter);
            builder.Property(c => c.TenantId).HasConversion(ulidConverter);

            builder.ToTable("Cattle");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TagNumber).HasMaxLength(50).IsRequired();
            builder.Property(c => c.Breed).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Gender).HasConversion<string>().HasMaxLength(10);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.FatherTagNumber).HasMaxLength(50).IsRequired(false);
            builder.Property(c => c.MotherTagNumber).HasMaxLength(50).IsRequired(false);

            builder.HasIndex(c => c.TagNumber).IsUnique();
            builder.HasIndex(c => new { c.TenantId, c.Status });

            builder.HasOne(c => c.Tenant)
                .WithMany(t => t.Cattle)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.HealthLogs).WithOne(h => h.Cattle).HasForeignKey(h => h.CattleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Vaccinations).WithOne(v => v.Cattle).HasForeignKey(v => v.CattleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.MilkLogs).WithOne(m => m.Cattle).HasForeignKey(m => m.CattleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Alerts).WithOne(a => a.Cattle).HasForeignKey(a => a.CattleId).OnDelete(DeleteBehavior.Cascade);


        }
    }
}