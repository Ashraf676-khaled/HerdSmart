// Application/Features/Vaccines/Commands/DeleteVaccine/DeleteVaccineCommandValidator.cs
using FluentValidation;

namespace Application.Features.Vaccines.Commands.DeleteVaccine;

public class DeleteVaccineCommandValidator : AbstractValidator<DeleteVaccineCommand>
{
    public DeleteVaccineCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}