using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.User.Commands.UpdateUser;
using Application.Features.User.Dtos;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        UserManager<AppUser> userManager,
        ITenantProvider tenantProvider,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<UserResponse> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null || user.TenantId != tenantId)
            throw new NotFoundException("User not found");

        user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
                throw new BadRequestException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User updated: {UserId}", user.Id);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email!,
            user.Role);
    }
}