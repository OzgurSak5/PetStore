using System.ComponentModel.DataAnnotations;
using PetStore.Domain.Enums;

namespace PetStore.Domain.DTOs;

public record CreatePetRequest(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,

    [Range(1, int.MaxValue)]
    int BreedId,

    DateOnly BirthDate,
    Gender Gender,

    [Range(0, 1000000)]
    decimal Price,
    bool IsVaccinated
);