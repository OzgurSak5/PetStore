using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface ISpeciesRepository
{
    Task<List<Species>> GetAllAsync();
    Task<Species?> GetByIdAsync(int id);
    Task<Species> AddAsync(Species species);
    Task UpdateAsync(Species species);
    Task DeleteAsync(Species species);
}