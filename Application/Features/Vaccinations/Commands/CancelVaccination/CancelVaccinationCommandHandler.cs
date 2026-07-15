using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccinations.Commands.CancelVaccination;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class CancelVaccinationCommandHandler : IRequestHandler<CancelVaccinationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CancelVaccinationCommandHandler> _logger;

    public CancelVaccinationCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<CancelVaccinationCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(
        CancelVaccinationCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var schedule = await _context.VaccinationSchedules
            .FirstOrDefaultAsync(
                s => s.Id == request.Id && s.TenantId == tenantId,
                cancellationToken);

        if (schedule is null)
        {
            _logger.LogWarning(
                "Cancel vaccination failed - Schedule {ScheduleId} not found", request.Id);
            throw new NotFoundException("Vaccination schedule not found");
        }

        if (schedule.Status == VaccinationStatus.Completed)
        {
            _logger.LogWarning(
                "Cancel vaccination blocked - Schedule {ScheduleId} is already Completed",
                request.Id);
            throw new BadRequestException("Cannot cancel a completed vaccination");
        }

        schedule.DeletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Vaccination schedule cancelled: {ScheduleId} - Cattle {CattleId}, Vaccine {VaccineId}",
            schedule.Id, schedule.CattleId, schedule.VaccineId);
    }
}