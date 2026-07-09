using Application.Features.User.Dtos;
using MediatR;

namespace Application.Features.User.Commands.UpdateUser
{
    public record UpdateUserCommand(
        Guid Id,
        string FullName,
        string? Password = null) : IRequest<UserResponse>;
}
