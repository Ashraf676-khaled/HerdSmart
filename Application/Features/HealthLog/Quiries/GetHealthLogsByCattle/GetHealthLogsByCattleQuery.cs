
using MediatR;

public record GetHealthLogsByCattleQuery(
    Ulid CattleId,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResult<HealthLogResponse>>;