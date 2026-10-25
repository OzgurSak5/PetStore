using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetStore.Domain.DTOs;
using PetStore.Domain.Entities;
using PetStore.Domain.Enums;
using PetStore.Domain.Exceptions;
using PetStore.Domain.Interfaces;
using PetStore.Domain.Settings;

namespace PetStore.Domain.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILoginAttemptLimiter _loginLimiter;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ILoginAttemptLimiter loginLimiter,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _loginLimiter = loginLimiter;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null || !_passwordHasher.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid current password.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
    }

    private async Task<string> CreateRefreshTokenAsync(User user)
    {
        var rawToken = _tokenService.GenerateRefreshToken();
        var now = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(rawToken),
            ExpiresAt = now.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedAt = now
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        return rawToken;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        if (_loginLimiter.IsBlocked(email))
        {
            _logger.LogWarning("Login blocked by rate limit. Email: {Email}", email);
            throw new TooManyRequestsException(
                "Too many login attempts. Please try again later.",
                _loginLimiter.RetryAfterSeconds);
        }

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _loginLimiter.RecordFailure(email);
            _logger.LogInformation("Failed login attempt. Email: {Email}", email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (accessToken, expiresInSeconds) = _tokenService.CreateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user);

        return new TokenResponse(accessToken, refreshToken, expiresInSeconds);
    }

    public async Task LogoutAsync(RefreshRequest request)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (stored is not null && stored.IsActive)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(stored);
        }
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest request)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (stored is null || !stored.IsActive || stored.User is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        stored.RevokedAt = DateTime.UtcNow;

        var (accessToken, expiresInSeconds) = _tokenService.CreateAccessToken(stored.User);
        var newRefreshToken = await CreateRefreshTokenAsync(stored.User);

        return new TokenResponse(accessToken, newRefreshToken, expiresInSeconds);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = EmailNormalizer.Normalize(request.Email);

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new ConflictException("Email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);

        return new AuthResponse(createdUser.Id, createdUser.Email, createdUser.Role.ToString());
    }
}