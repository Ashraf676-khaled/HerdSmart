using FluentValidation;

public class GetCattleHistoryQueryValidator : AbstractValidator<GetCattleHistoryQuery>
{
    public GetCattleHistoryQueryValidator()
    {
        RuleFor(x => x.CattleId)
            .NotEmpty().WithMessage("Cattle ID is required");
    }
}