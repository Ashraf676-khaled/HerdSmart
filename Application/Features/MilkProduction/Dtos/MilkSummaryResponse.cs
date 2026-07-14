public record MilkSummaryResponse(
    double TotalLiters,
    double AverageLitersPerDay,
    int TotalSessions,
    DateTimeOffset From,
    DateTimeOffset To);
