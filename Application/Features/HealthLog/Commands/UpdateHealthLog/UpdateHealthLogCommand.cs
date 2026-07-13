// Application/Features/HealthLogs/Commands/UpdateHealthLog/UpdateHealthLogCommand.cs
using MediatR;

namespace Application.Features.HealthLogs.Commands.UpdateHealthLog;

public record UpdateHealthLogCommand(
    Ulid Id,
    string Diagnosis,
    string TreatmentPlan,
    string? VetNotes) : IRequest<HealthLogResponse>;