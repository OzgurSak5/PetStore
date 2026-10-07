using PetStore.Domain.Entities;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IBreedRepository
{
    Task<Breed?> GetByIdAsync(int id);
    Task<Breed> AddAsync(Breed breed);
    Task<bool> ExistsBySpeciesIdAsync(int speciesId);
    Task UpdateAsync(Breed breed);
    Task DeleteAsync(Breed breed);
    Task<(List<Breed> Items, int TotalCount)> GetPagedAsync(BreedQueryParameters parameters);
}