using System.ComponentModel.DataAnnotations;
using PetStore.Domain.Enums;

namespace PetStore.Domain.DTOs;

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