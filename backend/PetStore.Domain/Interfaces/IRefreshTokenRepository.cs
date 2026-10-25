using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(int userId);
}