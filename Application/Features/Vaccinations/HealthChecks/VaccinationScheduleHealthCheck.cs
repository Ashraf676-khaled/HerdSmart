// Application/Features/Vaccinations/HealthChecks/VaccinationScheduleHealthCheck.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Features.Vaccinations.HealthChecks;

public class VaccinationScheduleHealthCheck(IApplicationDbContext context) : IHealthCheck
{
    private const int MaxAllowedOverdueSchedules = 15;
    private static readonly TimeSpan LongOverdueThreshold = TimeSpan.FromDays(30);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var overdueCount = await context.VaccinationSchedules
                .CountAsync(s => s.Status == VaccinationStatus.Overdue, cancellationToken);

            var longOverdueCutoff = DateTimeOffset.UtcNow - LongOverdueThreshold;

            var longOverdueCount = await context.VaccinationSchedules
                .CountAsync(s =>
                    s.Status == VaccinationStatus.Overdue &&
                    s.ScheduledDate < longOverdueCutoff,
                    cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["overdueCount"] = overdueCount,
                ["longOverdueCount"] = longOverdueCount
            };

            if (longOverdueCount > 0)
            {
                return HealthCheckResult.Degraded(
                    $"Operational Warning: {longOverdueCount} vaccination(s) have been overdue for more than {LongOverdueThreshold.TotalDays} days. Possible untreated cattle.",
                    data: data);
            }

            if (overdueCount > MaxAllowedOverdueSchedules)
            {
                return HealthCheckResult.Degraded(
                    $"Operational Warning: {overdueCount} vaccination schedules are currently overdue, exceeding the acceptable threshold ({MaxAllowedOverdueSchedules}).",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Vaccination scheduling is on track. {overdueCount} schedule(s) currently overdue (within acceptable range).",
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to execute Vaccination schedule health check due to a database exception.", ex);
        }
    }
}