using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.MilkLogs.Queries.GetTopProducers;

public class GetTopProducersQueryHandler
    : IRequestHandler<GetTopProducersQuery, IEnumerable<TopProducerResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetTopProducersQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<IEnumerable<TopProducerResponse>> Handle(
        GetTopProducersQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.MilkProductionLogs
            .Where(m => m.TenantId == tenantId)
            .AsQueryable();

        if (request.From.HasValue)
            query = query.Where(m => m.LoggedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(m => m.LoggedAt <= request.To.Value);

        // 1. Pull raw data as flat types to avoid translation issues
        var rawLogs = await query
            .Select(m => new
            {
                m.CattleId,
                CattleTagNumber = m.Cattle.TagNumber,
                m.AmountInLiters // This is float
            })
            .ToListAsync(cancellationToken);

        // 2. Perform GroupBy safely in-memory (AsEnumerable is implicit now)
        var topProducers = rawLogs
            .GroupBy(m => new { m.CattleId, m.CattleTagNumber })
            .Select(g => new TopProducerResponse(
                g.Key.CattleId,
                g.Key.CattleTagNumber,
                g.Sum(m => m.AmountInLiters), // Sums floats directly in C#
                g.Count()
            ))
            .OrderByDescending(x => x.TotalLiters)
            .Take(request.Top)
            .ToList();

        return topProducers;
    }
}