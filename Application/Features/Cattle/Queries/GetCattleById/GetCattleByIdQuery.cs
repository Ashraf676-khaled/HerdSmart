using Application.Features.Cattle.Dtos;
using MediatR;

public record GetCattleByIdQuery(Ulid Id) : IRequest<CattleResponse>;
