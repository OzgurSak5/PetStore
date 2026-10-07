using PetStore.Domain.DTOs;
using PetStore.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(int id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);

    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
}

public record CreateUserRequest(
    [Required]
    [EmailAddress]
    [StringLength(255)]
    string Email,

    [Required]
    [StringLength(255, MinimumLength = 8)]
    string Password,

    UserRole Role
);

public record UpdateUserRequest(
    [Required]
    [EmailAddress]
    [StringLength(255)]
    string Email,

    UserRole Role
);