// Application/Features/Vaccinations/Commands/AdministerVaccination/AdministerVaccinationCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Vaccinations.Commands.AdministerVaccination;

public class AdministerVaccinationCommandHandler
    : IRequestHandler<AdministerVaccinationCommand, VaccinationScheduleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AdministerVaccinationCommandHandler> _logger;

    public AdministerVaccinationCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<AdministerVaccinationCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<VaccinationScheduleResponse> Handle(
        AdministerVaccinationCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var schedule = await _context.VaccinationSchedules
            .Include(s => s.Cattle)
            .Include(s => s.Vaccine)
            .FirstOrDefaultAsync(
                s => s.Id == request.Id && s.TenantId == tenantId,
                cancellationToken);

        if (schedule is null)
            throw new NotFoundException("Vaccination schedule not found");

        if (schedule.Status == VaccinationStatus.Completed)
            throw new BadRequestException("This vaccination is already completed");

        var administeredDate = request.AdministeredDate ?? DateTimeOffset.UtcNow;

        schedule.AdministeredDate = administeredDate;
        schedule.Status = VaccinationStatus.Completed;

        // لو اللقاح محتاج جرعة منشطة - نعمل Schedule جديد تلقائي
        if (schedule.Vaccine.IntervalInDays.HasValue)
        {
            var nextSchedule = new VaccinationSchedule
            {
                TenantId = tenantId,
                CattleId = schedule.CattleId,
                VaccineId = schedule.VaccineId,
                ScheduledDate = administeredDate.AddDays(schedule.Vaccine.IntervalInDays.Value),
                Status = VaccinationStatus.Pending
            };

            await _context.VaccinationSchedules.AddAsync(nextSchedule, cancellationToken);

            _logger.LogInformation(
                "Booster schedule auto-created for CattleId {CattleId}, VaccineId {VaccineId}, due {ScheduledDate}",
                schedule.CattleId, schedule.VaccineId, nextSchedule.ScheduledDate);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
    "Vaccination administered: {ScheduleId} - Cattle {CattleId}, Vaccine {VaccineId}, on {AdministeredDate}",
    schedule.Id, schedule.CattleId, schedule.VaccineId, administeredDate);

        return new VaccinationScheduleResponse(
            schedule.Id, schedule.CattleId, schedule.Cattle.TagNumber,
            schedule.VaccineId, schedule.Vaccine.Name,
            schedule.ScheduledDate, schedule.AdministeredDate, schedule.Status);
    }
}