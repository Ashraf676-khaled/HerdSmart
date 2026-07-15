// Application/Features/Vaccinations/Queries/GetVaccinationsByCattle/GetVaccinationsByCattleQueryValidator.cs
using FluentValidation;

namespace Application.Features.Vaccinations.Queries.GetVaccinationsByCattle;

public class GetVaccinationsByCattleQueryValidator : AbstractValidator<GetVaccinationsByCattleQuery>
{
    public GetVaccinationsByCattleQueryValidator()
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