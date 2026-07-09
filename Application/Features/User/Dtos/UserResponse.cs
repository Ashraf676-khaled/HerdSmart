using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.User.Dtos
{
    public sealed record UserResponse
   (
        Guid Id ,
        string FullName,
        String Email,
        UserRole Role
   );
}
