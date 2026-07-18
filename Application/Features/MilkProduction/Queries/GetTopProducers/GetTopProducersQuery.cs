using MediatR;

public record GetTopProducersQuery(
    int Top = 5,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IRequest<IEnumerable<TopProducerResponse>>;