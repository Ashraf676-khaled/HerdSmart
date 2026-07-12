using Application.Features.Auth.Dtos;
using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.User.Commands.CreateUser
{
    public sealed record CreateUserCommand
        (
            string FullName,
            string Email,
            string Password,
            UserRole Role
        ) : IRequest<AuthResponse>;


}
