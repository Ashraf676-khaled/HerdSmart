// Application/Features/MilkLogs/Queries/GetAllMilkLogs/GetAllMilkLogsQuery.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.MilkLogs.Queries.GetAllMilkLogs;

public class GetAllMilkLogsQueryHandler
    : IRequestHandler<GetAllMilkLogsQuery, PaginatedResult<MilkLogResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetAllMilkLogsQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<MilkLogResponse>> Handle(
        GetAllMilkLogsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.MilkProductionLogs
            .Where(m => m.TenantId == tenantId)
            .AsQueryable();

        if (request.Shift.HasValue)
            query = query.Where(m => m.Shift == request.Shift.Value);

        if (request.From.HasValue)
            query = query.Where(m => m.LoggedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(m => m.LoggedAt <= request.To.Value);

        return await query
            .OrderByDescending(m => m.LoggedAt)
            .Select(m => new MilkLogResponse(
                m.Id,
                m.CattleId,
                m.Cattle.TagNumber,
                m.AmountInLiters,
                m.Shift,
                m.LoggedAt,
                m.CreatedBy))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}