namespace Application.Common.Interfaces
{
    public interface IRefreshTokenService
    {
        Task SaveRefreshTokenAsync(Guid userId, string token);
        Task<string?> GetRefreshTokenAsync(Guid userId);
        Task RevokeRefreshTokenAsync(Guid userId);

    }
}
