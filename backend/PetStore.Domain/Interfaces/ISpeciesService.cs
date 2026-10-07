using PetStore.Domain.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.Interfaces;

public interface ISpeciesService
{
    Task<List<SpeciesResponse>> GetAllAsync();
    Task<SpeciesResponse?> GetByIdAsync(int id);
    Task<SpeciesResponse> CreateAsync(CreateSpeciesRequest request);
    Task<SpeciesResponse> UpdateAsync(int id, UpdateSpeciesRequest request);
    Task DeleteAsync(int id);
}

public record CreateSpeciesRequest(
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string Name
);

public record UpdateSpeciesRequest(
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string Name
);