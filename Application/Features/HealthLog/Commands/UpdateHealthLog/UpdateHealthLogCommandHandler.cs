// Application/Features/HealthLogs/Commands/UpdateHealthLog/UpdateHealthLogCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.HealthLogs.Commands.UpdateHealthLog;

public class UpdateHealthLogCommandHandler
    : IRequestHandler<UpdateHealthLogCommand, HealthLogResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateHealthLogCommandHandler> _logger;

    public UpdateHealthLogCommandHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ILogger<UpdateHealthLogCommandHandler> logger)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<HealthLogResponse> Handle(
        UpdateHealthLogCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var healthLog = await _context.HealthLogs
            .Include(h => h.Cattle)
            .FirstOrDefaultAsync(
                h => h.Id == request.Id && h.TenantId == tenantId,
                cancellationToken);

        if (healthLog is null)
            throw new NotFoundException("Health log not found");

        // Business Rule - مش تقدر تعدل بعد 24 ساعة
        if (healthLog.CreatedAt < DateTimeOffset.UtcNow.AddHours(-24))
            throw new BadRequestException("Cannot update health log after 24 hours");

        healthLog.Diagnosis = request.Diagnosis;
        healthLog.TreatmentPlan = request.TreatmentPlan;
        healthLog.VetNotes = request.VetNotes;
        healthLog.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HealthLog updated: {HealthLogId}", healthLog.Id);

        return new HealthLogResponse(
            healthLog.Id,
            healthLog.CattleId,
            healthLog.Cattle.TagNumber,
            healthLog.Diagnosis,
            healthLog.TreatmentPlan,
            healthLog.VetNotes,
            healthLog.IsContagious,
            healthLog.CreatedAt);
    }
}