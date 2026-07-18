using Application.Features.Telemetry.Jobs;
using Hangfire;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Services.BackgroundJobs.Extensions;

public static class HangfireJobsExtensions
{
    public static void RegisterRecurringJobs(this IApplicationBuilder app)
    {

        RecurringJob.AddOrUpdate<MarkOverdueVaccinationsJob>(
            "mark-overdue-vaccinations",
            job => job.ExecuteAsync(),
            Cron.Daily(2, 0));

        RecurringJob.AddOrUpdate<AutoGenerateVaccinationSchedulesJob>(
            "auto-generate-vaccination-schedules",
            job => job.ExecuteAsync(),
            Cron.Daily(1, 0));

        RecurringJob.AddOrUpdate<CleanupResolvedTelemetryAlertsJob>(
    "cleanup-resolved-telemetry-alerts",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 3 1 * *");
    }
}