using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshAsync(RefreshRequest request);
    Task LogoutAsync(RefreshRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}