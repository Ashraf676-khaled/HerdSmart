
using HerdSmart.Domain.Enums;
using MediatR;

public record GetAllMilkLogsQuery(
    int Page = 1,
    int PageSize = 10,
    MilkShift? Shift = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IRequest<PaginatedResult<MilkLogResponse>>;