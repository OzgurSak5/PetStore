namespace PetStore.Domain.DTOs;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds
);