using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface ISpeciesService
{
    Task<List<SpeciesResponse>> GetAllAsync();
    Task<SpeciesResponse?> GetByIdAsync(int id);
    Task<SpeciesResponse> CreateAsync(CreateSpeciesRequest request);
    Task<SpeciesResponse> UpdateAsync(int id, UpdateSpeciesRequest request);
    Task DeleteAsync(int id);
}
