using FluentValidation;

public class GetUpcomingVaccinationsQueryValidator : AbstractValidator<GetUpcomingVaccinationsQuery>
{
    public GetUpcomingVaccinationsQueryValidator()
    {
        RuleFor(x => x.Days).GreaterThan(0).LessThanOrEqualTo(90);
    }
}