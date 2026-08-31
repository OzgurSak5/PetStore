using PetStore.Domain.Entities;
using PetStore.Domain.Enums;

namespace PetStore.Domain.Interfaces;

public interface IPetService
{
    Task<List<Pet>> GetAllAsync();
    Task<Pet?> GetByIdAsync(int id);
    Task<Pet> CreateAsync(CreatePetRequest request);
}

public record CreatePetRequest(
    string Name,
    int BreedId,
    DateOnly BirthDate,
    Gender Gender,
    decimal Price,
    bool IsVaccinated
);
