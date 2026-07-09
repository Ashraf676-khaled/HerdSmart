using Application.Features.Auth.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand
        (
        string RefreshToken,
        string AccessToken
        ):IRequest<AuthResponse>;
}
