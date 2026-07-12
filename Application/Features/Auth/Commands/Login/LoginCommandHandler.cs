using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Dtos;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;

        public LoginCommandHandler(UserManager<AppUser> userManager, IRefreshTokenService refreshTokenService
            ,ILogger<LoginCommandHandler> logger , IJwtService jwtService)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _logger = logger;
            _refreshTokenService= refreshTokenService;
        }
        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            //check email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password");
            //check password
            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if(!isValid)
                throw new UnauthorizedException("Invalid email or password");
            // 3. Generate Tokens
            var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);
            await _refreshTokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            _logger.LogInformation($"User logged in: {user.Id}");

            // 4. Return Response
            return new AuthResponse(
                accessToken,
                refreshToken,
                user.FullName,
                user.Role.ToString(),
                user.TenantId);
        }
    }
}


