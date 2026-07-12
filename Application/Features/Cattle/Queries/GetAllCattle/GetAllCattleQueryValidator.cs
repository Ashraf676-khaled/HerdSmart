// GetAllCattleQueryValidator
using FluentValidation;

public class GetAllCattleQueryValidator : AbstractValidator<GetAllCattleQuery>
{
    public GetAllCattleQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");
    }
}