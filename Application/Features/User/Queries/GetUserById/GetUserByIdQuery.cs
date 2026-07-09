using Application.Features.User.Dtos;
using MediatR;

namespace Application.Features.User.Queries.GetUserById
{
    public sealed record GetUserByIdQuery
    (Guid Id) : IRequest<UserResponse>;
}
