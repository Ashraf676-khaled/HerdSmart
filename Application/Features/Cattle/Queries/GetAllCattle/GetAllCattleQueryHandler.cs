// Application/Features/Cattle/Queries/GetAllCattle/GetAllCattleQuery.cs
using Application.Common.Interfaces;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.Cattle.Queries.GetAllCattle;
public class GetAllCattleQueryHandler
    : IRequestHandler<GetAllCattleQuery, PaginatedResult<CattleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMapper _mapper;

    public GetAllCattleQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        IMapper mapper)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<CattleResponse>> Handle(
        GetAllCattleQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.Cattle
            .Where(c => c.TenantId == tenantId)
            .AsQueryable();

        // Filter by Status
        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        // Search by TagNumber or Breed
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c =>
                c.TagNumber.Contains(request.Search) ||
                c.Breed.Contains(request.Search));

        return await query
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
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}