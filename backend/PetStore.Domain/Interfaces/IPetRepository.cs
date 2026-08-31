using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IPetRepository
{
    Task<List<Pet>> GetAllAsync();
    Task<Pet?> GetByIdAsync(int id);
    Task<Pet> AddAsync(Pet pet);
}