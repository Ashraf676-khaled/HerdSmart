// Application/Features/HealthLogs/Commands/CreateHealthLog/CreateHealthLogCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.HealthLog.Commands.CreateHealthLog;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.HealthLogs.Commands.CreateHealthLog;

public class CreateHealthLogCommandHandler
    : IRequestHandler<CreateHealthLogCommand, HealthLogResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateHealthLogCommandHandler> _logger;

    public CreateHealthLogCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IMapper mapper,
        ILogger<CreateHealthLogCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HealthLogResponse> Handle(
        CreateHealthLogCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        // 1. تحقق إن البقرة موجودة
        var cattle = await _context.Cattle
            .FirstOrDefaultAsync(
                c => c.Id == request.CattleId && c.TenantId == tenantId,
                cancellationToken);

        if (cattle is null)
            throw new NotFoundException("Cattle not found");

        // 2. Business Rule - مش تقدر تسجل لبقرة Dead
        if (cattle.Status == CattleStatus.Dead)
            throw new BadRequestException("Cannot add health log for dead cattle");

        // 3. اعمل HealthLog
        var healthLog = _mapper.Map<HerdSmart.Domain.Entities.HealthLog>(request);
        healthLog.TenantId = tenantId;

        // 4. Business Rule - لو معدي → عزل البقرة
        if (request.IsContagious && cattle.Status != CattleStatus.Isolated)
        {
            cattle.Status = CattleStatus.Isolated;
            cattle.UpdatedAt = DateTimeOffset.UtcNow;
            _logger.LogInformation(
                "Cattle {CattleId} isolated due to contagious disease", cattle.Id);
        }

        await _context.HealthLogs.AddAsync(healthLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HealthLog created: {HealthLogId}", healthLog.Id);

        return new HealthLogResponse(
            healthLog.Id,
            healthLog.CattleId,
            cattle.TagNumber,
            healthLog.Diagnosis,
            healthLog.TreatmentPlan,
            healthLog.VetNotes,
            healthLog.IsContagious,
            healthLog.CreatedAt);
    }
}