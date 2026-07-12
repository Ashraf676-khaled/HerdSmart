using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.User.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : IRequest;

}
