// Application/Features/Vaccinations/Queries/GetVaccinationsByCattle/GetVaccinationsByCattleQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vaccinations.Queries.GetVaccinationsByCattle;

public class GetVaccinationsByCattleQueryHandler
    : IRequestHandler<GetVaccinationsByCattleQuery, PaginatedResult<VaccinationScheduleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetVaccinationsByCattleQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<VaccinationScheduleResponse>> Handle(
        GetVaccinationsByCattleQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var cattleExists = await _context.Cattle
            .AnyAsync(c => c.Id == request.CattleId && c.TenantId == tenantId,
                cancellationToken);

        if (!cattleExists)
            throw new NotFoundException("Cattle not found");

        return await _context.VaccinationSchedules
            .Where(s => s.CattleId == request.CattleId && s.TenantId == tenantId)
            .OrderBy(s => s.ScheduledDate)
            .Select(s => new VaccinationScheduleResponse(
                s.Id, s.CattleId, s.Cattle.TagNumber,
                s.VaccineId, s.Vaccine.Name,
                s.ScheduledDate, s.AdministeredDate, s.Status))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}