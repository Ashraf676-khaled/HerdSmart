// Application/Common/Interfaces/IJwtService.cs
using HerdSmart.Domain.Entities;
using System.Security.Claims;

namespace Application.Common.Interfaces;

public interface IJwtService
{
    (string AccessToken, string RefreshToken) GenerateTokens(AppUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}