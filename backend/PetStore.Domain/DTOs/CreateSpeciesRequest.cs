using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record CreateSpeciesRequest(
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string Name
);