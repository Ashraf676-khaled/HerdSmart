using Application.Features.Cattle.Dtos;
using HerdSmart.Domain.Enums;
using MediatR;

public record UpdateCattleStatusCommand(
    Ulid Id,
    CattleStatus NewStatus) : IRequest<CattleResponse>;
