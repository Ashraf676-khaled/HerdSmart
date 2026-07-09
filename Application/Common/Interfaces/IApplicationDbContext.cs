// Application/Common/Interfaces/IApplicationDbContext.cs
using HerdSmart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<AppUser> Users { get; }
    DbSet<Cattle> Cattle { get; }
    DbSet<HealthLog> HealthLogs { get; }
    DbSet<VaccinationSchedule> VaccinationSchedules { get; }
    DbSet<Vaccine> Vaccines { get; }
    DbSet<MilkProductionLog> MilkProductionLogs { get; }
    DbSet<TelemetryAlert> TelemetryAlerts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}