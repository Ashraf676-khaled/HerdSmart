using Application.Features.Auth.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommand
    (
        string Email,
        string Password
    ):IRequest<AuthResponse>;
}
