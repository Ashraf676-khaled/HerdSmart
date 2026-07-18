// Application/Features/MilkLogs/Queries/GetMilkLogsByCattle/GetMilkLogsByCattleQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.MilkLogs.Queries.GetMilkLogsByCattle;

public class GetMilkLogsByCattleQueryHandler
    : IRequestHandler<GetMilkLogsByCattleQuery, PaginatedResult<MilkLogResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetMilkLogsByCattleQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<MilkLogResponse>> Handle(
        GetMilkLogsByCattleQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var cattleExists = await _context.Cattle
            .AnyAsync(c => c.Id == request.CattleId && c.TenantId == tenantId,
                cancellationToken);

        if (!cattleExists)
            throw new NotFoundException("Cattle not found");

        return await _context.MilkProductionLogs
            .Where(m => m.CattleId == request.CattleId && m.TenantId == tenantId)
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