using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IBreedService
{
    Task<List<BreedResponse>> GetAllAsync();
    Task<BreedResponse?> GetByIdAsync(int id);
    Task<BreedResponse> CreateAsync(CreateBreedRequest request);
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