// Application/Features/Cattle/Queries/GetCattleById/GetCattleByIdQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Cattle.Queries.GetCattleById;


public class GetCattleByIdQueryHandler : IRequestHandler<GetCattleByIdQuery, CattleResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetCattleByIdQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<CattleResponse> Handle(
        GetCattleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var cattle = await _context.Cattle
            .Where(c => c.Id == request.Id && c.TenantId == tenantId)
            .Select(c => new CattleResponse(
                c.Id,
                c.TagNumber,
                c.Breed,
                c.Gender,
                c.BirthDate,
                c.Status,
                c.FatherTagNumber,
                c.MotherTagNumber,
                c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (cattle is null)
            throw new NotFoundException("Cattle not found");

        return cattle;
    }
}