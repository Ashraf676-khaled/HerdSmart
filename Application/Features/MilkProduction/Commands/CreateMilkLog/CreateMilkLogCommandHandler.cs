// Application/Features/MilkLogs/Commands/CreateMilkLog/CreateMilkLogCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.MilkLogs.Commands.CreateMilkLog;

public class CreateMilkLogCommandHandler
    : IRequestHandler<CreateMilkLogCommand, MilkLogResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateMilkLogCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateMilkLogCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<CreateMilkLogCommandHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
        _mapper= mapper;
    }

    public async Task<MilkLogResponse> Handle(
        CreateMilkLogCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        // 1. تحقق من البقرة
        var cattle = await _context.Cattle
            .FirstOrDefaultAsync(
                c => c.Id == request.CattleId && c.TenantId == tenantId,
                cancellationToken);

        if (cattle is null)
            throw new NotFoundException("Cattle not found");

        // 2. Business Rule - Status
        if (cattle.Status is CattleStatus.Dead
            or CattleStatus.Dry
            or CattleStatus.Sick)
            throw new BadRequestException(
                $"Cannot log milk for cattle with status {cattle.Status}");

        // 3. Business Rule - نفس الـ Shift في نفس اليوم
        var loggedAt = request.LoggedAt ?? DateTimeOffset.UtcNow;
        var sameShiftExists = await _context.MilkProductionLogs
            .AnyAsync(m =>
                m.CattleId == request.CattleId &&
                m.Shift == request.Shift &&
                m.LoggedAt.Date == loggedAt.Date,
                cancellationToken);

        if (sameShiftExists)
            throw new ConflictException(
                $"Milk log already exists for {request.Shift} shift today");

        // 4. Create Log
        var milkLog = _mapper.Map<MilkProductionLog>(request);
        milkLog.TenantId = tenantId;

        await _context.MilkProductionLogs.AddAsync(milkLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("MilkLog created: {MilkLogId}", milkLog.Id);

        return new MilkLogResponse(
            milkLog.Id,
            milkLog.CattleId,
            cattle.TagNumber,
            milkLog.AmountInLiters,
            milkLog.Shift,
            milkLog.LoggedAt,
            milkLog.CreatedBy);
    }
}