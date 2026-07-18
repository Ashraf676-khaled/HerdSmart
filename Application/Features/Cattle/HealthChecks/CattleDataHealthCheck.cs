// Application/Features/Cattle/HealthChecks/CattleDataHealthCheck.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Features.Cattle.HealthChecks;

public class CattleDataHealthCheck(IApplicationDbContext context) : IHealthCheck
{
    private static readonly TimeSpan StaleSickThreshold = TimeSpan.FromDays(14);
    private const int MaxAllowedStaleSickCattle = 10;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context_, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoff = DateTimeOffset.UtcNow - StaleSickThreshold;

            var staleSickCount = await context.Cattle
                .Where(c => c.Status == CattleStatus.Sick
                    && c.UpdatedAt != null
                    && c.UpdatedAt < cutoff)
                .CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["staleSickCattleCount"] = staleSickCount
            };

            if (staleSickCount > MaxAllowedStaleSickCattle)
            {
                return HealthCheckResult.Degraded(
                    $"Data Integrity Warning: {staleSickCount} cattle have been marked Sick for over {StaleSickThreshold.TotalDays} days without updates. Possible stuck veterinarian workflow.",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Cattle health statuses are well-maintained. Currently, {staleSickCount} cattle are in Sick status within the acceptable time window.",
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to execute Cattle data health query due to a database exception.", ex);
        }
    }
}