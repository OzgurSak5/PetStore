using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);