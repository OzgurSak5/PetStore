using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;

namespace PetStore.Domain.Services;

public class SpeciesService : ISpeciesService
{
    private readonly ISpeciesRepository _speciesRepository;

    public SpeciesService(ISpeciesRepository speciesRepository)
    {
        _speciesRepository = speciesRepository;
    }

    public async Task<List<Species>> GetAllAsync()
    {
        return await _speciesRepository.GetAllAsync();
    }

    public async Task<Species?> GetByIdAsync(int id)
    {
        return await _speciesRepository.GetByIdAsync(id);
    }

    public async Task<Species> CreateAsync(string name)
    {
        var species = new Species { Name = name };
        return await _speciesRepository.AddAsync(species);
    }
}