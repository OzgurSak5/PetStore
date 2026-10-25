using System.IdentityModel.Tokens.Jwt;   // JwtSecurityToken, JwtSecurityTokenHandler, JwtRegisteredClaimNames
using System.Security.Claims;            // Claim
using System.Security.Cryptography;      // RandomNumberGenerator, SHA256
using System.Text;                       // Encoding
using Microsoft.Extensions.Options;      // IOptions
using Microsoft.IdentityModel.Tokens;    // SymmetricSecurityKey, SigningCredentials, SecurityAlgorithms
using PetStore.Domain.Entities;          // User
using PetStore.Domain.Interfaces;        // ITokenService
using PetStore.Domain.Settings;          // JwtSettings

namespace PetStore.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public (string token, int expiresInSeconds) CreateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, _jwtSettings.AccessTokenMinutes * 60);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}