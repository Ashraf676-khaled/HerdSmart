// Infrastructure/Services/BackgroundJobs/MarkOverdueVaccinationsJob.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.BackgroundJobs;

public class MarkOverdueVaccinationsJob
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MarkOverdueVaccinationsJob> _logger;

    public MarkOverdueVaccinationsJob(
        IApplicationDbContext context,
        ILogger<MarkOverdueVaccinationsJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("MarkOverdueVaccinationsJob started at {Time}", DateTimeOffset.UtcNow);

        var now = DateTimeOffset.UtcNow;

        var overdueSchedules = await _context.VaccinationSchedules
            .Where(s => s.Status == VaccinationStatus.Pending && s.ScheduledDate < now)
            .ToListAsync();

        if (overdueSchedules.Count == 0)
        {
            _logger.LogInformation("No overdue vaccinations found");
            return;
        }

        foreach (var schedule in overdueSchedules)
        {
            schedule.Status = VaccinationStatus.Overdue;
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "MarkOverdueVaccinationsJob completed - {Count} schedules marked as Overdue",
            overdueSchedules.Count);
    }
}