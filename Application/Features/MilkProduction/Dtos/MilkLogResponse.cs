using HerdSmart.Domain.Enums;
public record MilkLogResponse(
    Ulid Id,
    Ulid CattleId,
    string CattleTagNumber,
    double AmountInLiters,
    MilkShift Shift,
    DateTimeOffset LoggedAt,
    Guid? CreatedBy
    );