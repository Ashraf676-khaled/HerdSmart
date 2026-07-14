// Application/Features/MilkLogs/Commands/CreateMilkLog/CreateMilkLogCommandValidator.cs
using FluentValidation;
namespace Application.Features.MilkLogs.Commands.CreateMilkLog;

public class CreateMilkLogCommandValidator : AbstractValidator<CreateMilkLogCommand>
{
    public CreateMilkLogCommandValidator()
    {
        RuleFor(x => x.CattleId)
            .NotEmpty();

        RuleFor(x => x.AmountInLiters)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Amount seems unrealistic");

        RuleFor(x => x.Shift)
            .IsInEnum();

        RuleFor(x => x.LoggedAt)
            .Must(date => date is null || date <= DateTimeOffset.UtcNow)
            .WithMessage("LoggedAt cannot be in the future");
    }
}