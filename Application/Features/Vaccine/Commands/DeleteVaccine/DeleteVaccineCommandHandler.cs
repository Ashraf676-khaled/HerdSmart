// Application/Features/Vaccines/Commands/DeleteVaccine/DeleteVaccineCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Vaccines.Commands.DeleteVaccine;

public class DeleteVaccineCommandHandler : IRequestHandler<DeleteVaccineCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<DeleteVaccineCommandHandler> _logger;

    public DeleteVaccineCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<DeleteVaccineCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(
        DeleteVaccineCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var vaccine = await _context.Vaccines
            .FirstOrDefaultAsync(
                v => v.Id == request.Id && v.TenantId == tenantId,
                cancellationToken);

        if (vaccine is null)
        {
            _logger.LogWarning(
                "Delete vaccine failed - Vaccine {VaccineId} not found", request.Id);
            throw new NotFoundException("Vaccine not found");
        }

        // مش تقدر تشيل لقاح لو مرتبط بيه أي Schedule (سواء اتحقن أو لسه)
        var hasSchedules = await _context.VaccinationSchedules
            .AnyAsync(s => s.VaccineId == request.Id, cancellationToken);

        if (hasSchedules)
        {
            _logger.LogWarning(
                "Delete vaccine blocked - Vaccine {VaccineId} has linked schedules",
                request.Id);
            throw new BadRequestException(
                "Cannot delete a vaccine that has vaccination schedules linked to it");
        }

        _context.Vaccines.Remove(vaccine);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Vaccine deleted: {VaccineId} - {Name}", vaccine.Id, vaccine.Name);
    }
}