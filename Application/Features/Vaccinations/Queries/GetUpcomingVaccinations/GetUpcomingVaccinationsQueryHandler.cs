// Application/Features/Vaccinations/Queries/GetUpcomingVaccinations/GetUpcomingVaccinationsQuery.cs
using Application.Common.Interfaces;
using HerdSmart.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vaccinations.Queries.GetUpcomingVaccinations;

public class GetUpcomingVaccinationsQueryHandler
    : IRequestHandler<GetUpcomingVaccinationsQuery, IEnumerable<VaccinationScheduleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetUpcomingVaccinationsQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<IEnumerable<VaccinationScheduleResponse>> Handle(
        GetUpcomingVaccinationsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        var now = DateTimeOffset.UtcNow;
        var until = now.AddDays(request.Days);

        return await _context.VaccinationSchedules
            .Where(s =>
                s.TenantId == tenantId &&
                s.Status == VaccinationStatus.Pending &&
                s.ScheduledDate >= now &&
                s.ScheduledDate <= until)
            .OrderBy(s => s.ScheduledDate)
            .Select(s => new VaccinationScheduleResponse(
                s.Id, s.CattleId, s.Cattle.TagNumber,
                s.VaccineId, s.Vaccine.Name,
                s.ScheduledDate, s.AdministeredDate, s.Status))
            .ToListAsync(cancellationToken);
    }
}