using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IBreedRepository
{
    Task<List<Breed>> GetAllAsync();
    Task<Breed?> GetByIdAsync(int id);
    Task<Breed> AddAsync(Breed breed);
    Task<bool> ExistsBySpeciesIdAsync(int speciesId);
}