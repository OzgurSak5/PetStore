using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record RefreshRequest(
    [Required] string RefreshToken
);