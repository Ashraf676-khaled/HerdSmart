// Application/Features/Vaccines/Commands/UpdateVaccine/UpdateVaccineCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Vaccines.Commands.UpdateVaccine;

public class UpdateVaccineCommandHandler
    : IRequestHandler<UpdateVaccineCommand, VaccineResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateVaccineCommandHandler> _logger;


    public UpdateVaccineCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<UpdateVaccineCommandHandler>logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger=logger;
    }

    public async Task<VaccineResponse> Handle(
        UpdateVaccineCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var vaccine = await _context.Vaccines
            .FirstOrDefaultAsync(
                v => v.Id == request.Id && v.TenantId == tenantId,
                cancellationToken);

        if (vaccine is null)
            throw new NotFoundException("Vaccine not found");

        var nameExists = await _context.Vaccines
            .AnyAsync(v =>
                v.TenantId == tenantId &&
                v.Id != request.Id &&
                v.Name.ToLower() == request.Name.ToLower(),
                cancellationToken);

        if (nameExists)
            throw new ConflictException($"Vaccine '{request.Name}' already exists");

        vaccine.Name = request.Name;
        vaccine.TargetAgeInMonths = request.TargetAgeInMonths;
        vaccine.Dosage = request.Dosage;
        vaccine.IntervalInDays = request.IntervalInDays;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Vaccine updated: {VaccineId} - {Name}", vaccine.Id, vaccine.Name);

        return new VaccineResponse(
            vaccine.Id,
            vaccine.Name,
            vaccine.TargetAgeInMonths,
            vaccine.Dosage,
            vaccine.IntervalInDays);
    }
}