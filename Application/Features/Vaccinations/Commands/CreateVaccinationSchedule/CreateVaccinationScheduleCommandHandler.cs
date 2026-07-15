// Application/Features/Vaccinations/Commands/CreateVaccinationSchedule/CreateVaccinationScheduleCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Vaccinations.Commands.CreateVaccinationSchedule;

public class CreateVaccinationScheduleCommandHandler
    : IRequestHandler<CreateVaccinationScheduleCommand, VaccinationScheduleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateVaccinationScheduleCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateVaccinationScheduleCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<CreateVaccinationScheduleCommandHandler> logger,
        IMapper mapper
        )
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger= logger;
        _mapper = mapper;
    }

    public async Task<VaccinationScheduleResponse> Handle(
      CreateVaccinationScheduleCommand request,
      CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var cattle = await _context.Cattle
            .FirstOrDefaultAsync(
                c => c.Id == request.CattleId && c.TenantId == tenantId,
                cancellationToken);

        if (cattle is null)
        {
            _logger.LogWarning(
                "Vaccination schedule creation failed - Cattle {CattleId} not found",
                request.CattleId);
            throw new NotFoundException("Cattle not found");
        }

        if (cattle.Status == CattleStatus.Dead)
        {
            _logger.LogWarning(
                "Vaccination schedule creation blocked - Cattle {CattleId} is Dead",
                request.CattleId);
            throw new BadRequestException("Cannot schedule vaccination for dead cattle");
        }

        var vaccine = await _context.Vaccines
            .FirstOrDefaultAsync(
                v => v.Id == request.VaccineId && v.TenantId == tenantId,
                cancellationToken);

        if (vaccine is null)
            throw new NotFoundException("Vaccine not found");

        var activeScheduleExists = await _context.VaccinationSchedules
            .AnyAsync(s =>
                s.CattleId == request.CattleId &&
                s.VaccineId == request.VaccineId &&
                (s.Status == VaccinationStatus.Pending || s.Status == VaccinationStatus.Overdue),
                cancellationToken);

        if (activeScheduleExists)
        {
            _logger.LogWarning(
                "Vaccination schedule creation blocked - active schedule already exists for Cattle {CattleId}, Vaccine {VaccineId}",
                request.CattleId, request.VaccineId);
            throw new ConflictException(
                "An active schedule already exists for this cattle and vaccine");
        }

        var schedule = _mapper.Map<VaccinationSchedule>(request);
        schedule.TenantId = tenantId;
        schedule.Status = VaccinationStatus.Pending;

        await _context.VaccinationSchedules.AddAsync(schedule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Vaccination schedule created: {ScheduleId} - Cattle {CattleId}, Vaccine {VaccineId}, due {ScheduledDate}",
            schedule.Id, schedule.CattleId, schedule.VaccineId, schedule.ScheduledDate);

        return new VaccinationScheduleResponse(
            schedule.Id, cattle.Id, cattle.TagNumber,
            vaccine.Id, vaccine.Name,
            schedule.ScheduledDate, schedule.AdministeredDate, schedule.Status);
    }
}