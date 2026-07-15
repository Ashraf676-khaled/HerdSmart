// Application/Features/Vaccinations/Commands/AdministerVaccination/AdministerVaccinationCommandValidator.cs
using FluentValidation;

namespace Application.Features.Vaccinations.Commands.AdministerVaccination;

public class AdministerVaccinationCommandValidator
    : AbstractValidator<AdministerVaccinationCommand>
{
    public AdministerVaccinationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.AdministeredDate)
            .Must(date => date is null || date <= DateTimeOffset.UtcNow)
            .WithMessage("Administered date cannot be in the future");
    }
}