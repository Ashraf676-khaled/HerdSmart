// Features/User/Queries/GetAllUsers/GetAllUsersQueryValidator.cs
using FluentValidation;

namespace Application.Features.User.Queries.GetAllUsers;

public class GetAllUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => x.Search != null)
            .WithMessage("Search term is too long");
    }
}