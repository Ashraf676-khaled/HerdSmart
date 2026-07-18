// Application/Features/Vaccinations/Queries/GetAllVaccinations/GetAllVaccinationsQuery.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.Vaccinations.Queries.GetAllVaccinations;

public class GetAllVaccinationsQueryHandler
    : IRequestHandler<GetAllVaccinationsQuery, PaginatedResult<VaccinationScheduleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetAllVaccinationsQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<VaccinationScheduleResponse>> Handle(
        GetAllVaccinationsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.VaccinationSchedules
            .Where(s => s.TenantId == tenantId)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.From.HasValue)
            query = query.Where(s => s.ScheduledDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(s => s.ScheduledDate <= request.To.Value);

        return await query
            .OrderBy(s => s.ScheduledDate)
            .Select(s => new VaccinationScheduleResponse(
                s.Id, s.CattleId, s.Cattle.TagNumber,
                s.VaccineId, s.Vaccine.Name,
                s.ScheduledDate, s.AdministeredDate, s.Status))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}