// Infrastructure/Services/BackgroundJobs/AutoGenerateVaccinationSchedulesJob.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.BackgroundJobs;

public class AutoGenerateVaccinationSchedulesJob
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AutoGenerateVaccinationSchedulesJob> _logger;

    public AutoGenerateVaccinationSchedulesJob(
        IApplicationDbContext context,
        ILogger<AutoGenerateVaccinationSchedulesJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation(
            "AutoGenerateVaccinationSchedulesJob started at {Time}", DateTimeOffset.UtcNow);

        var today = DateTimeOffset.UtcNow;

        // كل الأبقار الحية اللي عندها Vaccine schedules موجودة أصلاً (عشان نستثنيهم بسهولة)
        var existingPairs = await _context.VaccinationSchedules
            .Select(s => new { s.CattleId, s.VaccineId })
            .ToListAsync();

        var existingPairsSet = existingPairs
            .Select(p => (p.CattleId, p.VaccineId))
            .ToHashSet();

        var activeCattle = await _context.Cattle
            .Where(c => c.Status != CattleStatus.Dead)
            .ToListAsync();

        var allVaccines = await _context.Vaccines.ToListAsync();

        var newSchedules = new List<VaccinationSchedule>();

        foreach (var cattle in activeCattle)
        {
            var ageInMonths = CalculateAgeInMonths(cattle.BirthDate, today);

            // اللقاحات المتاحة لنفس الـ Tenant بتاع البقرة (أو Global لو TenantId = null)
            var eligibleVaccines = allVaccines.Where(v =>
                (v.TenantId == null || v.TenantId == cattle.TenantId) &&
                ageInMonths >= v.TargetAgeInMonths);

            foreach (var vaccine in eligibleVaccines)
            {
                if (existingPairsSet.Contains((cattle.Id, vaccine.Id)))
                    continue; // فيه Schedule أصلاً - متتعملش تاني

                var scheduledDate = cattle.BirthDate.AddMonths(vaccine.TargetAgeInMonths);

                newSchedules.Add(new VaccinationSchedule
                {
                    TenantId = cattle.TenantId,
                    CattleId = cattle.Id,
                    VaccineId = vaccine.Id,
                    ScheduledDate = scheduledDate,
                    Status = VaccinationStatus.Pending
                });

                // نضيفها للـ Set عشان لو نفس البقرة/اللقاح اتكرر في اللوب (مش هيحصل هنا، بس حماية إضافية)
                existingPairsSet.Add((cattle.Id, vaccine.Id));
            }
        }

        if (newSchedules.Count == 0)
        {
            _logger.LogInformation("No new vaccination schedules to generate");
            return;
        }

        await _context.VaccinationSchedules.AddRangeAsync(newSchedules);
        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "AutoGenerateVaccinationSchedulesJob completed - {Count} new schedules created",
            newSchedules.Count);
    }

    private static int CalculateAgeInMonths(DateTimeOffset birthDate, DateTimeOffset today)
    {
        var months = (today.Year - birthDate.Year) * 12 + (today.Month - birthDate.Month);

        if (today.Day < birthDate.Day)
            months--;

        return months;
    }
}