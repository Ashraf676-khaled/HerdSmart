using HerdSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class VaccinationScheduleConfiguration : IEntityTypeConfiguration<VaccinationSchedule>
    {
        public void Configure(EntityTypeBuilder<VaccinationSchedule> builder)
        {
            builder.ToTable("VaccinationSchedules");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.ScheduledDate).IsRequired();
            builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(v => v.CattleId);
            builder.HasIndex(v => v.VaccineId);

            builder.HasOne(v => v.Cattle)
                .WithMany(c => c.Vaccinations)
                .HasForeignKey(v => v.CattleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.Vaccine)
                .WithMany(vac => vac.Schedules)
                .HasForeignKey(v => v.VaccineId)
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