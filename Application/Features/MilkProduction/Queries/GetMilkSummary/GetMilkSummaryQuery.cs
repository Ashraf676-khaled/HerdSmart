using MediatR;

public record GetMilkSummaryQuery(
    DateTimeOffset From,
    DateTimeOffset To) : IRequest<MilkSummaryResponse>;