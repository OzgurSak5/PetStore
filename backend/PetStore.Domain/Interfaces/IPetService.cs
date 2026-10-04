using PetStore.Domain.DTOs;
using PetStore.Domain.Enums;

namespace PetStore.Domain.Interfaces;

public interface IPetService
{
    Task<List<PetResponse>> GetAllAsync();
    Task<PetResponse?> GetByIdAsync(int id);
    Task<PetResponse> CreateAsync(CreatePetRequest request);

    Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request);
    Task DeleteAsync(int id);
}


public record CreatePetRequest(
    string Name,
    int BreedId,
    DateOnly BirthDate,
    Gender Gender,
    decimal Price,
    bool IsVaccinated
);

public record UpdatePetRequest(
    string Name,
    int BreedId,
    DateOnly BirthDate,
    Gender Gender,
    decimal Price,
    bool IsVaccinated,
    PetStatus Status
);
