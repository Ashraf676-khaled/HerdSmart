using Application.Common.Interfaces;
using Application.Features.Auth.Dtos;
using AutoMapper;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Common.Exceptions;

namespace Application.Features.User.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, AuthResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly ITenantProvider _tenantProvider;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<CreateUserCommandHandler> _logger;
        public CreateUserCommandHandler(UserManager<AppUser> userManager, IJwtService jwtService, IMapper mapper,
            ITenantProvider tenantProvider,IRefreshTokenService refreshTokenService, ILogger<CreateUserCommandHandler> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _mapper = mapper;
            _tenantProvider = tenantProvider;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<AuthResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            //check email 
            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new ConflictException("Email alredy exist");
            //Get tenantid
            var tenantId = _tenantProvider.GetTenantId();
            //create user
            var user = _mapper.Map<AppUser>(request);
            user.TenantId = tenantId;
            user.Role = request.Role;
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new BadRequestException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            _logger.LogInformation($"User created: {user.Id} with role {user.Role}");
            //Generate tokens
            var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);
            await _refreshTokenService.SaveRefreshTokenAsync(user.Id, refreshToken);
            //response
            return new AuthResponse(
            accessToken,
            refreshToken,
            user.FullName,
            user.Role.ToString(),
            tenantId);
        }
    }
}
