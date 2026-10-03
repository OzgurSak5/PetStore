using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Services;

public class SpeciesService : ISpeciesService
{
    private readonly ISpeciesRepository _speciesRepository;

    public SpeciesService(ISpeciesRepository speciesRepository)
    {
        _speciesRepository = speciesRepository;
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

    public async Task<SpeciesResponse> CreateAsync(string name)
    {
        var species = new Species { Name = name };
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