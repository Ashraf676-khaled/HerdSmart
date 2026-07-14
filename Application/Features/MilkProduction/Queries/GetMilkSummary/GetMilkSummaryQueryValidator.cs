// GetMilkSummaryQueryValidator
using FluentValidation;

public class GetMilkSummaryQueryValidator : AbstractValidator<GetMilkSummaryQuery>
{
    public GetMilkSummaryQueryValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty().WithMessage("From date is required");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("To date is required")
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("To must be after From");
    }
}