// Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Dtos;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        UserManager<AppUser> userManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. جيب الـ Claims من الـ Access Token المنتهي
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
            throw new UnauthorizedException("Invalid access token");


        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Invalid token claims");

        // 3. جيب اليوزر
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new UnauthorizedException("User not found");

        // 4. تحقق من الـ Refresh Token في Redis
        var savedToken = await _refreshTokenService.GetRefreshTokenAsync(userId);
        if (savedToken is null || savedToken != request.RefreshToken)
            throw new UnauthorizedException("Invalid or expired refresh token");

        // 5. Generate Tokens جدد
        var (newAccessToken, newRefreshToken) = _jwtService.GenerateTokens(user);
        await _refreshTokenService.SaveRefreshTokenAsync(userId, newRefreshToken);

        _logger.LogInformation("Token refreshed for user: {UserId}", userId);

        // 6. Return Response
        return new AuthResponse(
            newAccessToken,
            newRefreshToken,
            user.FullName,
            user.Role.ToString(),
            user.TenantId);
    }
}