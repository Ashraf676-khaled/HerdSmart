// Infrastructure/HealthChecks/HangfireJobsHealthCheck.cs
using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class HangfireJobsHealthCheck : IHealthCheck
{
    private const int MaxAllowedFailedJobs = 3;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();

        var failedCount = monitoringApi.FailedCount();
        var processingCount = monitoringApi.ProcessingCount();

        var data = new Dictionary<string, object>
        {
            ["failedJobs"] = failedCount,
            ["processingJobs"] = processingCount
        };

        if (failedCount > MaxAllowedFailedJobs)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Hangfire background jobs failure threshold exceeded. Found {failedCount} failed jobs (Max allowed: {MaxAllowedFailedJobs}).",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Hangfire jobs are healthy. Currently processing: {processingCount} job(s), failed: {failedCount} job(s).",
            data));
    }
}