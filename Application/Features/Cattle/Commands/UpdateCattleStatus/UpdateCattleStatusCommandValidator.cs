using FluentValidation;

public class UpdateCattleStatusCommandValidator
    : AbstractValidator<UpdateCattleStatusCommand>
{
    public UpdateCattleStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cattle ID is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid status");
    }
}