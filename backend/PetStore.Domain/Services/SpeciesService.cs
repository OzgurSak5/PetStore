using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;
using PetStore.Domain.Exceptions;

namespace PetStore.Domain.Services;

public class SpeciesService : ISpeciesService
{
    private readonly ISpeciesRepository _speciesRepository;
    private readonly IBreedRepository _breedRepository;

    public SpeciesService(ISpeciesRepository speciesRepository, IBreedRepository breedRepository)
    {
        _speciesRepository = speciesRepository;
        _breedRepository = breedRepository;
    }

    public async Task<SpeciesResponse> UpdateAsync(int id, UpdateSpeciesRequest request)
    {
        var species = await _speciesRepository.GetByIdAsync(id);

        if (species == null)
        {
            throw new NotFoundException("Species not found.");
        }

        species.Name = request.Name;
        await _speciesRepository.UpdateAsync(species);

        return MapToResponse(species);
    }

    public async Task DeleteAsync(int id)
    {
        var species = await _speciesRepository.GetByIdAsync(id);

        if (species == null)
        {
            throw new NotFoundException("Species not found.");
        }

        var hasBreeds = await _breedRepository.ExistsBySpeciesIdAsync(id);
        if (hasBreeds)
        {
            throw new ConflictException("Cannot delete species with associated breeds.");
        }

        await _speciesRepository.DeleteAsync(species);
    }

    public async Task<List<SpeciesResponse>> GetAllAsync()
    {
        var speciesList = await _speciesRepository.GetAllAsync();
        return speciesList.Select(MapToResponse).ToList();
    }

    public async Task<SpeciesResponse?> GetByIdAsync(int id)
    {
        var species = await _speciesRepository.GetByIdAsync(id);
        return species != null ? MapToResponse(species) : null;
    }

    public async Task<SpeciesResponse> CreateAsync(CreateSpeciesRequest request)
    {
        var species = new Species { Name = request.Name };
        var createdSpecies = await _speciesRepository.AddAsync(species);
        return MapToResponse(createdSpecies);
    }


    private static SpeciesResponse MapToResponse(Species species)
    {
        return new SpeciesResponse(
            Id: species.Id,
            Name: species.Name
        );
    }
}