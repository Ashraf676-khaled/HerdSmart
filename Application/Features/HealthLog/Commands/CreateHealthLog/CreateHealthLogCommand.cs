// Application/Features/HealthLogs/Commands/CreateHealthLog/CreateHealthLogCommand.cs
using MediatR;

namespace Application.Features.HealthLog.Commands.CreateHealthLog;

public record CreateHealthLogCommand(
    Ulid CattleId,
    string Diagnosis,
    string TreatmentPlan,
    string? VetNotes,
    bool IsContagious) : IRequest<HealthLogResponse>;