public record TopProducerResponse(
    Ulid CattleId,
    string TagNumber,
    double TotalLiters,
    int TotalSessions);