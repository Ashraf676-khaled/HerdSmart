// Application/Features/HealthLogs/Queries/GetHealthLogsByCattle/GetHealthLogsByCattleQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.HealthLogs.Queries.GetHealthLogsByCattle;

public class GetHealthLogsByCattleQueryHandler
    : IRequestHandler<GetHealthLogsByCattleQuery, PaginatedResult<HealthLogResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetHealthLogsByCattleQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<HealthLogResponse>> Handle(
        GetHealthLogsByCattleQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        // تحقق إن البقرة موجودة
        var cattleExists = await _context.Cattle
            .AnyAsync(c => c.Id == request.CattleId && c.TenantId == tenantId,
                cancellationToken);

        if (!cattleExists)
            throw new NotFoundException("Cattle not found");

        return await _context.HealthLogs
            .Where(h => h.CattleId == request.CattleId && h.TenantId == tenantId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HealthLogResponse(
                h.Id,
                h.CattleId,
                h.Cattle.TagNumber,
                h.Diagnosis,
                h.TreatmentPlan,
                h.VetNotes,
                h.IsContagious,
                h.CreatedAt))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}