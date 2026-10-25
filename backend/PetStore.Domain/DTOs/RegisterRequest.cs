using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);