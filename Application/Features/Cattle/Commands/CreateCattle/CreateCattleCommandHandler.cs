// Application/Features/Cattle/Commands/CreateCattle/CreateCattleCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Cattle.Commands.CreateCattle;

public class CreateCattleCommandHandler : IRequestHandler<CreateCattleCommand, CattleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCattleCommandHandler> _logger;

    public CreateCattleCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IMapper mapper,
        ILogger<CreateCattleCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CattleResponse> Handle(
        CreateCattleCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        // 1. TagNumber unique per Tenant
        if (await _context.Cattle.AnyAsync(
            c => c.TagNumber == request.TagNumber && c.TenantId == tenantId,
            cancellationToken))
            throw new ConflictException($"TagNumber {request.TagNumber} already exists");

        // 2. Create Cattle
        var cattle = _mapper.Map<HerdSmart.Domain.Entities.Cattle>(request);
        cattle.TenantId = tenantId;
        cattle.Status = CattleStatus.Active;

        await _context.Cattle.AddAsync(cattle, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cattle created: {CattleId}", cattle.Id);

        return _mapper.Map<CattleResponse>(cattle);
    }
}