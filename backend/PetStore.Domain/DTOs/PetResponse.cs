namespace PetStore.Domain.DTOs;

public record PetResponse(
    int Id,
    string Name,
    string Gender,
    string Status,
    int BreedId,
    string BreedName,
    string SpeciesName,
    DateOnly BirthDate,
    int Age,
    decimal Price,
    bool IsVaccinated,
    DateTime CreatedAt
);