// HerdSmart.Infrastructure/Configurations/TenantConfiguration.cs
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HerdSmart.Infrastructure.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.Plan)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Index
            builder.HasIndex(t => t.Name);
            var ulidConverter = new ValueConverter<Ulid, string>(
                  v => v.ToString(),
                  v => Ulid.Parse(v));
            builder.Property(c => c.Id).HasConversion(ulidConverter);


        }
    }
}