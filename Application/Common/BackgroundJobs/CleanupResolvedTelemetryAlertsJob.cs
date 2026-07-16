// Application/Features/Telemetry/Jobs/CleanupResolvedTelemetryAlertsJob.cs
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Telemetry.Jobs;

public class CleanupResolvedTelemetryAlertsJob(
    IApplicationDbContext context,
    ILogger<CleanupResolvedTelemetryAlertsJob> logger)
{
    private const int RetentionMonths = 3;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddMonths(-RetentionMonths);

        var alertsToDelete = await context.TelemetryAlerts
            .Where(a => a.IsResolved && a.ResolvedAt != null && a.ResolvedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        if (alertsToDelete.Count == 0)
        {
            logger.LogInformation("Telemetry cleanup job: no resolved alerts to delete.");
            return;
        }

        context.TelemetryAlerts.RemoveRange(alertsToDelete);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Telemetry cleanup job: deleted {Count} resolved alerts older than {CutoffDate}.",
            alertsToDelete.Count, cutoffDate);
    }
}