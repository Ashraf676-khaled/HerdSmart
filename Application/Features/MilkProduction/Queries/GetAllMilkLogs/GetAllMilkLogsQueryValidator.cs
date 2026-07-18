// GetAllMilkLogsQueryValidator
using FluentValidation;

public class GetAllMilkLogsQueryValidator : AbstractValidator<GetAllMilkLogsQuery>
{
    public GetAllMilkLogsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("To must be after From");
    }
}