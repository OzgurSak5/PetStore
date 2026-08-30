using PetStore.Domain.Interfaces;
using PetStore.Domain.Entities;

namespace PetStore.Domain.Services;

public class BreedService : IBreedService
{
    private readonly IBreedRepository _breedRepository;

    public BreedService(IBreedRepository breedRepository)
    {
        _breedRepository = breedRepository;
    }

    public async Task<List<Breed>> GetAllAsync()
    {
        return await _breedRepository.GetAllAsync();
    }

    public async Task<Breed?> GetByIdAsync(int id)
    {
        return await _breedRepository.GetByIdAsync(id);
    }

    public async Task<Breed> CreateAsync(CreateBreedRequest request)
    {
        var breed = new Breed
        {
            Name = request.Name,
            SpeciesId = request.SpeciesId,
            EnergyLevel = request.EnergyLevel,
            NoiseLevel = request.NoiseLevel,
            SpaceRequirements = request.SpaceRequirement,
            AppetiteLevel = request.AppetiteLevel,
            GoodWithChildren = request.GoodWithChildren,
            GoodWithOtherPets = request.GoodWithOtherPets,
            GroomingNeeds = request.GroomingNeed
        };

        return await _breedRepository.AddAsync(breed);
    }
}