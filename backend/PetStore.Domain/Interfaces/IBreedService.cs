using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IBreedService
{
    Task<List<BreedResponse>> GetAllAsync();
    Task<BreedResponse?> GetByIdAsync(int id);
    Task<BreedResponse> CreateAsync(CreateBreedRequest request);
    Task<BreedResponse> UpdateAsync(int id, UpdateBreedRequest request);
    Task DeleteAsync(int id);
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

public record UpdateBreedRequest(
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