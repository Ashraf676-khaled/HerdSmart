// Application/Features/MilkLogs/Commands/UpdateMilkLog/UpdateMilkLogCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.MilkLogs.Commands.UpdateMilkLog;

public class UpdateMilkLogCommandHandler
    : IRequestHandler<UpdateMilkLogCommand, MilkLogResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateMilkLogCommandHandler> _logger;

    public UpdateMilkLogCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<UpdateMilkLogCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<MilkLogResponse> Handle(
        UpdateMilkLogCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var milkLog = await _context.MilkProductionLogs
            .Include(m => m.Cattle)
            .FirstOrDefaultAsync(
                m => m.Id == request.Id && m.TenantId == tenantId,
                cancellationToken);

        if (milkLog is null)
            throw new NotFoundException("Milk log not found");
        // Business Rule - مفيش عامل هيعدل على شغل التانى

        var currentUserId = _tenantProvider.GetUserId();
        var userRole = _tenantProvider.GetUserRole();

        if (userRole == "Worker" && milkLog.CreatedBy != currentUserId)
            throw new ForbiddenException("You can only update your own milk logs");

        // Business Rule - مش تقدر تعدل بعد 24 ساعة
        if (milkLog.LoggedAt < DateTimeOffset.UtcNow.AddHours(-24))
            throw new BadRequestException("Cannot update milk log after 24 hours");

        // Business Rule - لو بدلت الـ Shift تأكد مش موجودة
        if (milkLog.Shift != request.Shift)
        {
            var shiftExists = await _context.MilkProductionLogs
                .AnyAsync(m =>
                    m.CattleId == milkLog.CattleId &&
                    m.Shift == request.Shift &&
                    m.LoggedAt.Date == milkLog.LoggedAt.Date &&
                    m.Id != request.Id,
                    cancellationToken);

            if (shiftExists)
                throw new ConflictException(
                    $"Milk log already exists for {request.Shift} shift today");
        }

        milkLog.AmountInLiters = request.AmountInLiters;
        milkLog.Shift = request.Shift;
        milkLog.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("MilkLog updated: {MilkLogId}", milkLog.Id);

        return new MilkLogResponse(
            milkLog.Id,
            milkLog.CattleId,
            milkLog.Cattle.TagNumber,
            milkLog.AmountInLiters,
            milkLog.Shift,
            milkLog.LoggedAt,
            milkLog.CreatedBy);
    }
}