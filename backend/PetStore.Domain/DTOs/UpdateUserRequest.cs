using System.ComponentModel.DataAnnotations;
using PetStore.Domain.Enums;

namespace PetStore.Domain.DTOs;

public record UpdateUserRequest(
    [Required]
    [EmailAddress]
    [StringLength(255)]
    string Email,

    UserRole Role
);