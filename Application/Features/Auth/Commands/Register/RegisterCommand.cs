using Application.Features.Auth.Dtos;
using MediatR;

namespace Application.Features.Auth.Commands.Register
{
    public sealed record RegisterCommand 
    (
        string FarmName,
        string OwnerName,
        string Email ,
        string Password
    ):IRequest<AuthResponse>;
}
