// Users/Queries/GetUserById/GetUserByIdQuery.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.User.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserResponse>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetUserByIdQueryHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<UserResponse> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var user = await _context.Users
            .Where(u => u.TenantId == tenantId && u.Id == request.Id&&u.LockoutEnd==null)
            .Select(u => new UserResponse(
                u.Id,
                u.FullName,
                u.Email!,
                u.Role))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new NotFoundException("User not found");

        return user;
    }
}