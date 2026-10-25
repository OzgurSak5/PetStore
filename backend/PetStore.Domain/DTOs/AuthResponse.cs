namespace PetStore.Domain.DTOs;

public record AuthResponse(
    int UserId,
    string Email,
    string Role
);