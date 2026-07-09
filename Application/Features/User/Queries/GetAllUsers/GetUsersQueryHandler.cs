// Users/Queries/GetUsers/GetUsersQuery.cs
using Application.Common.Interfaces;
using Application.Features.User.Dtos;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;



public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, PaginatedResult<UserResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetUsersQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PaginatedResult<UserResponse>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.Users
            .Where(u => u.TenantId == tenantId&&u.LockoutEnd==null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u =>
                u.FullName.Contains(request.Search) ||
                u.Email!.Contains(request.Search));

        return await query
            .Select(u => new UserResponse(
                u.Id,
                u.FullName,
                u.Email!,
                u.Role))
            .AsPaginationAsync(request.Page, request.PageSize, cancellationToken);
    }
}