using PetStore.Domain.Interfaces;
using PetStore.Domain.Entities;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Services;

public class BreedService : IBreedService
{
    private readonly IBreedRepository _breedRepository;
    private readonly ISpeciesRepository _speciesRepository;

    public BreedService(IBreedRepository breedRepository, ISpeciesRepository speciesRepository)
    {
        _breedRepository = breedRepository;
        _speciesRepository = speciesRepository;
    }

    public async Task<List<BreedResponse>> GetAllAsync()
    {
        var breeds = await _breedRepository.GetAllAsync();
        return breeds.Select(MapToResponse).ToList();
    }

    public async Task<BreedResponse?> GetByIdAsync(int id)
    {
        var breed = await _breedRepository.GetByIdAsync(id);
        return breed != null ? MapToResponse(breed) : null;
    }

    public async Task<BreedResponse> CreateAsync(CreateBreedRequest request)
    {
        var species = await _speciesRepository.GetByIdAsync(request.SpeciesId);

        if (species == null)
        {
            throw new InvalidOperationException("Species not found.");
        }

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

        await _breedRepository.AddAsync(breed);

        var createdBreed = await _breedRepository.GetByIdAsync(breed.Id);
        return MapToResponse(createdBreed!);
    }

    private static BreedResponse MapToResponse(Breed breed)
    {
        return new BreedResponse(
            Id: breed.Id,
            Name: breed.Name,
            SpeciesId: breed.SpeciesId,
            SpeciesName: breed.Species?.Name ?? string.Empty,
            EnergyLevel: breed.EnergyLevel,
            NoiseLevel: breed.NoiseLevel,
            SpaceRequirement: breed.SpaceRequirements,
            AppetiteLevel: breed.AppetiteLevel,
            GoodWithChildren: breed.GoodWithChildren,
            GoodWithOtherPets: breed.GoodWithOtherPets,
            GroomingNeed: breed.GroomingNeeds
        );
    }
}