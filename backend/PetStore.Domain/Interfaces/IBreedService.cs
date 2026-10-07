using PetStore.Domain.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.Interfaces;

public interface IBreedService
{
    Task<PagedResult<BreedResponse>> GetPagedAsync(BreedQueryParameters parameters);
    Task<BreedResponse?> GetByIdAsync(int id);
    Task<BreedResponse> CreateAsync(CreateBreedRequest request);
    Task<BreedResponse> UpdateAsync(int id, UpdateBreedRequest request);
    Task DeleteAsync(int id);
}

public record CreateBreedRequest(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,

    [Range(1, int.MaxValue)]
    int SpeciesId,

    [Range(1, 5)]
    int EnergyLevel,
    [Range(1, 5)]
    int NoiseLevel,
    [Range(1, 5)]
    int SpaceRequirement,
    [Range(1, 5)]
    int AppetiteLevel,
    [Range(1, 5)]
    int GoodWithChildren,
    [Range(1, 5)]
    int GoodWithOtherPets,
    [Range(1, 5)]
    int GroomingNeed
);

public record UpdateBreedRequest(
    [Required]
    [StringLength(100, MinimumLength = 2)]
    string Name,

    [Range(1, int.MaxValue)]
    int SpeciesId,

    [Range(1, 5)]
    int EnergyLevel,
    [Range(1, 5)]
    int NoiseLevel,
    [Range(1, 5)]
    int SpaceRequirement,
    [Range(1, 5)]
    int AppetiteLevel,
    [Range(1, 5)]
    int GoodWithChildren,
    [Range(1, 5)]
    int GoodWithOtherPets,
    [Range(1, 5)]
    int GroomingNeed
);