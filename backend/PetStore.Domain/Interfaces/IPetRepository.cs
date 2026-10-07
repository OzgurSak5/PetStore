using PetStore.Domain.Entities;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(int id);
    Task<Pet> AddAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task<bool> ExistsByBreedIdAsync(int breedId);
    Task<(List<Pet> Items, int TotalCount)> GetPagedAsync(PetQueryParameters parameters);
}