using Application.Common.Interfaces;
using Application.Features.Auth.Dtos;
using HerdSmart.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using HerdSmart.Domain.Enums;
namespace Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IMapper _mapper;
        public RegisterCommandHandler (IApplicationDbContext context,ILogger<RegisterCommandHandler> logger
            ,IJwtService jwtService , UserManager<AppUser> userManager
            ,IRefreshTokenService refreshTokenService,IMapper mapper)
        {
            _context= context;
            _userManager= userManager;
            _jwtService= jwtService;
            _refreshTokenService= refreshTokenService;
            _logger = logger;
            _mapper= mapper;
        }
        public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            //check user if exist

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                throw new ConflictException("Email already exsit");
            //check farm name
            if (await _context.Tenants.AnyAsync(t => t.Name == request.FarmName, cancellationToken))
                throw new ConflictException("Farm name already exist");
            //create tenant
            var tenant = _mapper.Map<Tenant>(request);
            await _context.Tenants.AddAsync(tenant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            //create owner
            var owner = _mapper.Map<AppUser>(request);
            owner.TenantId = tenant.Id;
            owner.Role = UserRole.Owner;
            var result = await _userManager.CreateAsync(owner, request.Password);
            if (!result.Succeeded)
                throw new BadRequestException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            _logger.LogInformation($"Owner created:{owner.Id}");
            //generatetokens 
            var (accessToken, refreshToken) = _jwtService.GenerateTokens(owner);
            await _refreshTokenService.SaveRefreshTokenAsync(owner.Id,refreshToken);
            //response
            return new AuthResponse
                (accessToken,
                refreshToken,
                owner.FullName,
                owner.Role.ToString(),
                tenant.Id
                );
        }
               
    }
}

