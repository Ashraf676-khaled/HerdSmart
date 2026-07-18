using MediatR;

public record GetMilkLogsByCattleQuery(
    Ulid CattleId,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResult<MilkLogResponse>>;