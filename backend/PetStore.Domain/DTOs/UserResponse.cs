namespace PetStore.Domain.DTOs;

public record UserResponse(
    int Id,
    string Email,
    string Role
);