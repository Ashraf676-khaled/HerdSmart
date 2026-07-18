// Application/Features/MilkLogs/HealthChecks/MilkProductionHealthCheck.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Features.MilkLogs.HealthChecks;

public class MilkProductionHealthCheck(IApplicationDbContext context) : IHealthCheck
{
    private static readonly TimeSpan MissedLoggingThreshold = TimeSpan.FromDays(2);
    private const int MaxAllowedMissingCattle = 10;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoff = DateTimeOffset.UtcNow - MissedLoggingThreshold;

            // 1. حساب إجمالي البقر النشط مباشرة في الداتابيز
            var activeCattleCount = await context.Cattle
                .CountAsync(c => c.Status == CattleStatus.Active, cancellationToken);

            if (activeCattleCount == 0)
            {
                return HealthCheckResult.Healthy("No active cattle to track for milk logging.");
            }

            // 2. حساب عدد البقر النشط اللي سجل لبن فعلاً في آخر يومين (Database-side evaluation)
            var cattleWithRecentLogCount = await context.Cattle
                .Where(c => c.Status == CattleStatus.Active &&
                            context.MilkProductionLogs.Any(m => m.CattleId == c.Id && m.LoggedAt >= cutoff))
                .CountAsync(cancellationToken);

            // 3. الحسبة بسيطة وجاهزة
            var missingCount = activeCattleCount - cattleWithRecentLogCount;

            var data = new Dictionary<string, object>
            {
                ["activeCattleCount"] = activeCattleCount,
                ["missingMilkLogCount"] = missingCount
            };

            if (missingCount > MaxAllowedMissingCattle)
            {
                return HealthCheckResult.Degraded(
                    $"Operational Warning: {missingCount} active cattle have no milk production records in the last {MissedLoggingThreshold.TotalDays} days. Possible missed logging by farm staff.",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Milk production logging is consistent. Only {missingCount} active cattle are missing recent logs.",
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to execute MilkProduction health check due to a database exception.", ex);
        }
    }
}