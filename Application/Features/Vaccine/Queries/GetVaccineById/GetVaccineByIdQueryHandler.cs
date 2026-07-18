// Application/Features/Vaccines/Queries/GetVaccineById/GetVaccineByIdQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vaccines.Queries.GetVaccineById;


public class GetVaccineByIdQueryHandler
    : IRequestHandler<GetVaccineByIdQuery, VaccineResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetVaccineByIdQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<VaccineResponse> Handle(
        GetVaccineByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var vaccine = await _context.Vaccines
            .FirstOrDefaultAsync(
                v => v.Id == request.Id && v.TenantId == tenantId,
                cancellationToken);

        if (vaccine is null)
            throw new NotFoundException("Vaccine not found");

        return new VaccineResponse(
            vaccine.Id, vaccine.Name, vaccine.TargetAgeInMonths,
            vaccine.Dosage, vaccine.IntervalInDays);
    }
}