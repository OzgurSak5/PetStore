namespace PetStore.Domain.DTOs;

public record FavoriteResponse(
    int PetId,
    string PetName,
    string BreedName,
    string SpeciesName,
    decimal Price,
    string Status,
    DateTime CreatedAt
);