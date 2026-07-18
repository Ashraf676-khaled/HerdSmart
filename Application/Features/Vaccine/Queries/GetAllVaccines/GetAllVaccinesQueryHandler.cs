// Application/Features/Vaccines/Queries/GetAllVaccines/GetAllVaccinesQuery.cs
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Vaccines.Queries.GetAllVaccines;

public class GetAllVaccinesQueryHandler
    : IRequestHandler<GetAllVaccinesQuery, PaginatedResult<VaccineResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetAllVaccinesQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<VaccineResponse>> Handle(
        GetAllVaccinesQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        return await _context.Vaccines
            .Where(v => v.TenantId == tenantId)
            .OrderBy(v => v.Name)
            .Select(v => new VaccineResponse(
                v.Id, v.Name, v.TargetAgeInMonths, v.Dosage, v.IntervalInDays))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}