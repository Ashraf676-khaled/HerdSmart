// Application/Features/MilkLogs/Commands/UpdateMilkLog/UpdateMilkLogCommandValidator.cs
using FluentValidation;

namespace Application.Features.MilkLogs.Commands.UpdateMilkLog;

public class UpdateMilkLogCommandValidator : AbstractValidator<UpdateMilkLogCommand>
{
    public UpdateMilkLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.AmountInLiters)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Amount seems unrealistic");

        RuleFor(x => x.Shift)
            .IsInEnum();
    }
}