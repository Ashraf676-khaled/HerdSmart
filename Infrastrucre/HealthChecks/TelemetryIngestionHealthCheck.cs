// Application/Features/Telemetry/HealthChecks/TelemetryIngestionHealthCheck.cs
using Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Application.Features.Telemetry.HealthChecks;

public class TelemetryIngestionHealthCheck(IHeartbeatTracker heartbeatTracker) : IHealthCheck
{
    private static readonly TimeSpan MaxSilenceWindow = TimeSpan.FromHours(1);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lastSeen = await heartbeatTracker.GetLastSeenAsync("global", cancellationToken);

        if (lastSeen is null)
        {
            return HealthCheckResult.Degraded("No IoT telemetry data has been received yet.");
        }

        var silenceDuration = DateTimeOffset.UtcNow - lastSeen.Value;

        if (silenceDuration > MaxSilenceWindow)
        {
            return HealthCheckResult.Unhealthy(
                $"IoT telemetry ingestion has stopped. No data received for {silenceDuration.TotalMinutes:F0} minutes (Last seen: {lastSeen:u}).");
        }

        return HealthCheckResult.Healthy(
            $"IoT telemetry ingestion is active. Last data received {silenceDuration.TotalMinutes:F0} minutes ago.");
    }
}