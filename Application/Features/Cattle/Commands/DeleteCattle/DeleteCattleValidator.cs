using FluentValidation;

public class DeleteCattleCommandValidator : AbstractValidator<DeleteCattleCommand>
{
    public DeleteCattleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cattle ID is required");
    }
}