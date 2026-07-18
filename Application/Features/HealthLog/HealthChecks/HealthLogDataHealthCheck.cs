// Application/Features/HealthLogs/HealthChecks/HealthLogDataHealthCheck.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Features.HealthLogs.HealthChecks;

public class HealthLogDataHealthCheck(IApplicationDbContext context) : IHealthCheck
{
    private static readonly TimeSpan StaleIsolationThreshold = TimeSpan.FromDays(7);
    private const int MaxAllowedStaleIsolatedCattle = 5;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoff = DateTimeOffset.UtcNow - StaleIsolationThreshold;

            var staleIsolatedCount = await context.Cattle
                .Where(c => c.Status == CattleStatus.Isolated
                    && c.UpdatedAt != null
                    && c.UpdatedAt < cutoff)
                .CountAsync(cancellationToken);

            var data = new Dictionary<string, object> { ["staleIsolatedCattleCount"] = staleIsolatedCount };

            if (staleIsolatedCount > MaxAllowedStaleIsolatedCattle)
            {
                return HealthCheckResult.Degraded(
                    $"Data Integrity Warning: {staleIsolatedCount} cattle have remained in Isolated status for over {StaleIsolationThreshold.TotalDays} days without follow-up. Possible untreated contagious cases.",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Cattle isolation data is up to date. Only {staleIsolatedCount} cattle are currently isolated beyond threshold.",
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to execute HealthLog data health check due to a database exception.", ex);
        }
    }
}