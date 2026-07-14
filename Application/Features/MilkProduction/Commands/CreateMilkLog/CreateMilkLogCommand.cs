// Application/Features/MilkLogs/Commands/CreateMilkLog/CreateMilkLogCommand.cs
using MediatR;
using HerdSmart.Domain.Enums;

namespace Application.Features.MilkLogs.Commands.CreateMilkLog;

public record CreateMilkLogCommand(
    Ulid CattleId,
    double AmountInLiters,
    MilkShift Shift,
    DateTimeOffset? LoggedAt) : IRequest<MilkLogResponse>;