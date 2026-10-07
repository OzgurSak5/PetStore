using PetStore.Domain.DTOs;
using PetStore.Domain.Enums;
using System.ComponentModel.DataAnnotations;


namespace PetStore.Domain.Interfaces;

public interface IPetService
{
    Task<PetResponse?> GetByIdAsync(int id);
    Task<PetResponse> CreateAsync(CreatePetRequest request);
    Task<PagedResult<PetResponse>> GetPagedAsync(PetQueryParameters parameters);
    Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request);
    Task DeleteAsync(int id);
}


public record CreatePetRequest(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,

    [Range(1, int.MaxValue)]
    int BreedId,

    DateOnly BirthDate,
    Gender Gender,
    
    [Range(0, 1000000)]
    decimal Price,
    bool IsVaccinated
);

public record UpdatePetRequest(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,

    [Range(1, int.MaxValue)]
    int BreedId,
    DateOnly BirthDate,
    Gender Gender,

    [Range(0, 1000000)]
    decimal Price,
    bool IsVaccinated,
    PetStatus Status
);
