using PetStore.Domain.Entities;
using PetStore.Domain.Enums;

namespace PetStore.Domain.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(CreateUserRequest request);
}

public record CreateUserRequest(
    string Email,
    string Password,
    UserRole Role
);