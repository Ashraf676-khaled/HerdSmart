using FluentValidation;

public class GetCattleByIdQueryValidator : AbstractValidator<GetCattleByIdQuery>
{
    public GetCattleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cattle ID is required");
    }
}