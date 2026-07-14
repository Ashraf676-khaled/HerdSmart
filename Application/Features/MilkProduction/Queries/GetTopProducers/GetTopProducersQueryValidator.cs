// GetTopProducersQueryValidator
using FluentValidation;

public class GetTopProducersQueryValidator : AbstractValidator<GetTopProducersQuery>
{
    public GetTopProducersQueryValidator()
    {
        RuleFor(x => x.Top)
            .GreaterThan(0).WithMessage("Top must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("Top cannot exceed 20");
    }
}