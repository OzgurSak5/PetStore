namespace PetStore.Domain.DTOs;

public record BreedResponse(
    int Id,
    string Name,
    int SpeciesId,
    string SpeciesName,
    int EnergyLevel,
    int NoiseLevel,
    int SpaceRequirement,
    int AppetiteLevel,
    int GoodWithChildren,
    int GoodWithOtherPets,
    int GroomingNeed
);