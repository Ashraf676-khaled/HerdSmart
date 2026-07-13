// Application/Features/HealthLogs/Commands/CreateHealthLog/CreateHealthLogCommandValidator.cs
using Application.Features.HealthLog.Commands.CreateHealthLog;
using FluentValidation;

namespace Application.Features.HealthLogs.Commands.CreateHealthLog;

public class CreateHealthLogCommandValidator
    : AbstractValidator<CreateHealthLogCommand>
{
    public CreateHealthLogCommandValidator()
    {
        RuleFor(x => x.CattleId)
            .NotEmpty().WithMessage("Cattle ID is required");

        RuleFor(x => x.Diagnosis)
            .NotEmpty().WithMessage("Diagnosis is required")
            .MaximumLength(500).WithMessage("Diagnosis cannot exceed 500 characters");

        RuleFor(x => x.TreatmentPlan)
            .NotEmpty().WithMessage("Treatment plan is required")
            .MaximumLength(1000).WithMessage("Treatment plan cannot exceed 1000 characters");

        RuleFor(x => x.VetNotes)
            .MaximumLength(500).WithMessage("Vet notes cannot exceed 500 characters")
            .When(x => x.VetNotes is not null);
    }
}