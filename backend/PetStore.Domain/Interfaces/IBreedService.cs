using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IBreedService
{
    Task<PagedResult<BreedResponse>> GetPagedAsync(BreedQueryParameters parameters);
    Task<BreedResponse?> GetByIdAsync(int id);
    Task<BreedResponse> CreateAsync(CreateBreedRequest request);
    Task<BreedResponse> UpdateAsync(int id, UpdateBreedRequest request);
    Task DeleteAsync(int id);
}
