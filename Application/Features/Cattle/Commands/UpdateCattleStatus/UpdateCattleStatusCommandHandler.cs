// Application/Features/Cattle/Commands/UpdateCattleStatus/UpdateCattleStatusCommand.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class UpdateCattleStatusCommandHandler
    : IRequestHandler<UpdateCattleStatusCommand, CattleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCattleStatusCommandHandler> _logger;

    public UpdateCattleStatusCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IMapper mapper,
        ILogger<UpdateCattleStatusCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CattleResponse> Handle(
        UpdateCattleStatusCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var cattle = await _context.Cattle
            .FirstOrDefaultAsync(
                c => c.Id == request.Id && c.TenantId == tenantId,
                cancellationToken);

        if (cattle is null)
            throw new NotFoundException("Cattle not found");

        // Business Rule — مش تقدر تغير Status لو Dead
        if (cattle.Status == CattleStatus.Dead)
            throw new BadRequestException("Cannot change status of dead cattle");

        // Business Rule — مش تقدر تعمل Sold لو Sick
        if (cattle.Status == CattleStatus.Sick && request.NewStatus == CattleStatus.Sold)
            throw new BadRequestException("Cannot sell sick cattle");

        var oldStatus = cattle.Status;
        cattle.Status = request.NewStatus;
        cattle.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cattle {CattleId} status changed from {OldStatus} to {NewStatus}",
            cattle.Id, oldStatus, request.NewStatus);

        return _mapper.Map<CattleResponse>(cattle);
    }
}