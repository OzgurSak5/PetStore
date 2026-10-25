using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record ChangePasswordRequest(
    [Required] string OldPassword,
    [Required, MinLength(8)] string NewPassword
);