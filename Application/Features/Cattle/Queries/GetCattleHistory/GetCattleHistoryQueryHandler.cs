// Application/Features/Cattle/Queries/GetCattleHistory/GetCattleHistoryQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cattle.Queries.GetCattleHistory;


public class GetCattleHistoryQueryHandler
    : IRequestHandler<GetCattleHistoryQuery, CattleHistoryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetCattleHistoryQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<CattleHistoryResponse> Handle(
     GetCattleHistoryQuery request,
     CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        // 1. check cattle
        var exists = await _context.Cattle
            .AnyAsync(c => c.Id == request.CattleId && c.TenantId == tenantId, cancellationToken);

        if (!exists)
            throw new NotFoundException("Cattle not found");

        // 2. Health Logs
        var rawHealthLogs = await _context.HealthLogs
            .Where(h => h.CattleId == request.CattleId && h.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken); 

        var healthLogs = rawHealthLogs
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HealthLogDto(h.Id, h.Diagnosis, h.TreatmentPlan, h.VetNotes, h.CreatedAt))
            .ToList();

        // 3. Milk Logs  
        var rawMilkLogs = await _context.MilkProductionLogs
            .Where(m => m.CattleId == request.CattleId && m.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var milkLogs = rawMilkLogs
            .OrderByDescending(m => m.LoggedAt)
            .Select(m => new MilkLogDto(m.Id, m.AmountInLiters, m.Shift, m.LoggedAt))
            .ToList();

        // 4. Vaccinations
        var rawVaccinations = await _context.VaccinationSchedules
            .Include(v => v.Vaccine)
            .Where(v => v.CattleId == request.CattleId && v.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var vaccinations = rawVaccinations
            .OrderByDescending(v => v.ScheduledDate)
            .Select(v => new VaccinationDto(
                v.Id,
                v.Vaccine?.Name ?? "Unknown",
                v.ScheduledDate,
                v.AdministeredDate,
                v.Status))
            .ToList();

        return new CattleHistoryResponse(healthLogs, milkLogs, vaccinations);
    }
}