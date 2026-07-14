// Application/Features/MilkLogs/Commands/UpdateMilkLog/UpdateMilkLogCommand.cs
using MediatR;
using HerdSmart.Domain.Enums;

namespace Application.Features.MilkLogs.Commands.UpdateMilkLog;

public record UpdateMilkLogCommand(
    Ulid Id,
    double AmountInLiters,
    MilkShift Shift) : IRequest<MilkLogResponse>;