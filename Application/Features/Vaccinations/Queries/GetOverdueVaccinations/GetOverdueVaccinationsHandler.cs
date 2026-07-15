// Application/Features/Vaccinations/Queries/GetOverdueVaccinations/GetOverdueVaccinationsQuery.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.Vaccinations.Queries.GetOverdueVaccinations;

public class GetOverdueVaccinationsQueryHandler
    : IRequestHandler<GetOverdueVaccinationsQuery, PaginatedResult<VaccinationScheduleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetOverdueVaccinationsQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<VaccinationScheduleResponse>> Handle(
        GetOverdueVaccinationsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        return await _context.VaccinationSchedules
            .Where(s => s.TenantId == tenantId && s.Status == VaccinationStatus.Overdue)
            .OrderBy(s => s.ScheduledDate)
            .Select(s => new VaccinationScheduleResponse(
                s.Id, s.CattleId, s.Cattle.TagNumber,
                s.VaccineId, s.Vaccine.Name,
                s.ScheduledDate, s.AdministeredDate, s.Status))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}