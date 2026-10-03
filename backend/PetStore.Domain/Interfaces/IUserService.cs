using PetStore.Domain.DTOs;
using PetStore.Domain.Enums;

namespace PetStore.Domain.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(int id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}

public record CreateUserRequest(
    string Email,
    string Password,
    UserRole Role
);