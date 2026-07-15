using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Vaccines;
using Application.Features.Vaccines.Commands.CreateVaccine;
using AutoMapper;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class CreateVaccineCommandHandler
    : IRequestHandler<CreateVaccineCommand, VaccineResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateVaccineCommandHandler> _logger;

    public CreateVaccineCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IMapper mapper,
        ILogger<CreateVaccineCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VaccineResponse> Handle(
        CreateVaccineCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var nameExists = await _context.Vaccines
            .AnyAsync(v =>
                v.TenantId == tenantId &&
                v.Name.ToLower() == request.Name.ToLower(),
                cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning(
                "Vaccine creation failed - duplicate name '{Name}' for Tenant {TenantId}",
                request.Name, tenantId);
            throw new ConflictException($"Vaccine '{request.Name}' already exists");
        }

        var vaccine = _mapper.Map<Vaccine>(request);
        vaccine.TenantId = tenantId;

        await _context.Vaccines.AddAsync(vaccine, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Vaccine created: {VaccineId} - {Name}", vaccine.Id, vaccine.Name);

        return _mapper.Map<VaccineResponse>(vaccine);
    }
}