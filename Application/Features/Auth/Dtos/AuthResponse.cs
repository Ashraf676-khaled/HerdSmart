namespace Application.Features.Auth.Dtos
{
    public record AuthResponse
     (
     string AccessToken,
     string RefreshToken,
     string FullName,
     string Role,
     Ulid TenantId
     );
}
