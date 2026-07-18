using Application.Common.Interfaces;
using HerdSmart.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace HerdSmart.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid> ,IApplicationDbContext 
    {
        private readonly ITenantProvider? _tenantProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Cattle> Cattle { get; set; }
        public DbSet<HealthLog> HealthLogs { get; set; }
        public DbSet<VaccinationSchedule> VaccinationSchedules { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<MilkProductionLog> MilkProductionLogs { get; set; }
        public DbSet<TelemetryAlert> TelemetryAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);



            // 1. طبق الـ Converter الـ  أول حاجة
            var ulidConverter = new ValueConverter<Ulid, string>(
              v => v.ToString(),
              v => Ulid.Parse(v));
            // 2. AppUser يدوياً بعد الـ Converter
            builder.Entity<AppUser>(entity =>
            {
                entity.Property(u => u.TenantId)
                    .HasConversion(ulidConverter)
                    .IsRequired();

                entity.Property(u => u.FullName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.TenantId);

                entity.HasOne(u => u.Tenant)
                    .WithMany(t => t.Users)
                    .HasForeignKey(u => u.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. Soft Delete Filter
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, "DeletedAt");
                    var comparison = Expression.Equal(prop, Expression.Constant(null));
                    var lambda = Expression.Lambda(comparison, parameter);
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // 4. Configurations في الآخر
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider?.IsAuthenticated() == true
                ? _tenantProvider.GetTenantId()
                : (Ulid?)null;

            // 1. Get the UserId as Guid? directly since CreatedBy is Guid?
            var userId = _tenantProvider?.IsAuthenticated() == true
                ? _tenantProvider.GetUserId()
                : (Guid?)null;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                        entry.Entity.CreatedBy = userId; // Safe assignment without conversion!

                        if (tenantId.HasValue)
                        {
                            var prop = entry.Entity.GetType().GetProperty("TenantId");
                            prop?.SetValue(entry.Entity, tenantId.Value);
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}