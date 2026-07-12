using Application.Features.Cattle.Dtos;
using HerdSmart.Domain.Enums;
using MediatR;

public record GetAllCattleQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    CattleStatus? Status = null) : IRequest<PaginatedResult<CattleResponse>>;
