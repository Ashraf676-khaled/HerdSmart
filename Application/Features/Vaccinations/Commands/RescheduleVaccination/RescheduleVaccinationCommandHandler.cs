// Application/Features/Vaccinations/Commands/RescheduleVaccination/RescheduleVaccinationCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vaccinations.Commands.RescheduleVaccination;

public class RescheduleVaccinationCommandHandler
    : IRequestHandler<RescheduleVaccinationCommand, VaccinationScheduleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RescheduleVaccinationCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<VaccinationScheduleResponse> Handle(
        RescheduleVaccinationCommand request,
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
            throw new BadRequestException("Cannot reschedule a completed vaccination");

        schedule.ScheduledDate = request.NewScheduledDate;

        // لو كانت Overdue وترجعله يوم في المستقبل، ترجع Pending تاني
        if (schedule.Status == VaccinationStatus.Overdue &&
            request.NewScheduledDate > DateTimeOffset.UtcNow)
        {
            schedule.Status = VaccinationStatus.Pending;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new VaccinationScheduleResponse(
            schedule.Id, schedule.CattleId, schedule.Cattle.TagNumber,
            schedule.VaccineId, schedule.Vaccine.Name,
            schedule.ScheduledDate, schedule.AdministeredDate, schedule.Status);
    }
}