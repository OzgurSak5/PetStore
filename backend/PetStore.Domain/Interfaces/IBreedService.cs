using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IBreedService
{
    Task<List<Breed>> GetAllAsync();
    Task<Breed?> GetByIdAsync(int id);
    Task<Breed> CreateAsync(CreateBreedRequest request);
}

public record CreateBreedRequest(
    string Name,
    int SpeciesId,
    int EnergyLevel,
    int NoiseLevel,
    int SpaceRequirement,
    int AppetiteLevel,
    int GoodWithChildren,
    int GoodWithOtherPets,
    int GroomingNeed
);