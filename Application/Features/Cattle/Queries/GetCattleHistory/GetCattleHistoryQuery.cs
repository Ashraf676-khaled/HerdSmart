using MediatR;

public record GetCattleHistoryQuery(Ulid CattleId) : IRequest<CattleHistoryResponse>;
