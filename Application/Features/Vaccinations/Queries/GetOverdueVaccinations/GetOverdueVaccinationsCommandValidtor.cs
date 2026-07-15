// Application/Features/Vaccinations/Queries/GetOverdueVaccinations/GetOverdueVaccinationsQueryValidator.cs
using FluentValidation;

namespace Application.Features.Vaccinations.Queries.GetOverdueVaccinations;

public class GetOverdueVaccinationsQueryValidator : AbstractValidator<GetOverdueVaccinationsQuery>
{
    public GetOverdueVaccinationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}