using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface ITokenService
{
    (string token, int expiresInSeconds) CreateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}