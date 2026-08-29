using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface ISpeciesService
{
    Task<List<Species>> GetAllAsync();
    Task<Species?> GetByIdAsync(int id);
    Task<Species> CreateAsync(string name);
}