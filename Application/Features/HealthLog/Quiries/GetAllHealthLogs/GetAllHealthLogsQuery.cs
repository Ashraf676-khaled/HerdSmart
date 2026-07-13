using MediatR;

public record GetAllHealthLogsQuery(
    int Page = 1,
    int PageSize = 10,
    bool? IsContagious = null) : IRequest<PaginatedResult<HealthLogResponse>>;