// Application/Features/MilkLogs/Queries/GetMilkSummary/GetMilkSummaryQuery.cs
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.MilkLogs.Queries.GetMilkSummary;

public class GetMilkSummaryQueryHandler
    : IRequestHandler<GetMilkSummaryQuery, MilkSummaryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetMilkSummaryQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<MilkSummaryResponse> Handle( GetMilkSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var logs = await _context.MilkProductionLogs
            .Where(m => m.TenantId == tenantId &&
                        m.LoggedAt >= request.From &&
                        m.LoggedAt <= request.To)
            .ToListAsync(cancellationToken);

        var totalDays = (request.To - request.From).Days + 1;

        return new MilkSummaryResponse(
            TotalLiters: logs.Sum(m => m.AmountInLiters),
            AverageLitersPerDay: totalDays > 0
                ? logs.Sum(m => m.AmountInLiters) / totalDays
                : 0,
            TotalSessions: logs.Count,
            From: request.From,
            To: request.To);
    }
}