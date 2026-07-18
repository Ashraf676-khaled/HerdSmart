using FluentValidation;

// GetMilkLogsByCattleQueryValidator
public class GetMilkLogsByCattleQueryValidator
    : AbstractValidator<GetMilkLogsByCattleQuery>
{
    public GetMilkLogsByCattleQueryValidator()
    {
        RuleFor(x => x.CattleId)
            .NotEmpty().WithMessage("Cattle ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}