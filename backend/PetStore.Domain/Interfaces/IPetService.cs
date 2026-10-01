using PetStore.Domain.DTOs;
using PetStore.Domain.Enums;

namespace PetStore.Domain.Interfaces;

public interface IPetService
{
    Task<List<PetResponse>> GetAllAsync();
    Task<PetResponse?> GetByIdAsync(int id);
    Task<PetResponse> CreateAsync(CreatePetRequest request);
}

public record CreatePetRequest(
    string Name,
    int BreedId,
    DateOnly BirthDate,
    Gender Gender,
    decimal Price,
    bool IsVaccinated
);
