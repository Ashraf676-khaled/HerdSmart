// Application/Features/HealthLogs/Queries/GetAllHealthLogs/GetAllHealthLogsQuery.cs
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.HealthLogs.Queries.GetAllHealthLogs;

public class GetAllHealthLogsQueryHandler
    : IRequestHandler<GetAllHealthLogsQuery, PaginatedResult<HealthLogResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetAllHealthLogsQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<HealthLogResponse>> Handle(
        GetAllHealthLogsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.HealthLogs
            .Where(h => h.TenantId == tenantId)
            .AsQueryable();

        if (request.IsContagious.HasValue)
            query = query.Where(h => h.IsContagious == request.IsContagious.Value);

        return await query
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