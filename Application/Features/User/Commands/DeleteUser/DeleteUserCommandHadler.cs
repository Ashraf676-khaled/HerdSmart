// Users/Commands/DeleteUser/DeleteUserCommand.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.User.Commands.DeleteUser;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Commands.DeleteUser;


public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        UserManager<AppUser> userManager,
        ITenantProvider tenantProvider,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null || user.TenantId != tenantId)
            throw new NotFoundException("User not found");

        // Soft Delete عن طريق Lockout
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        _logger.LogInformation("User deleted: {UserId}", user.Id);
    }
}