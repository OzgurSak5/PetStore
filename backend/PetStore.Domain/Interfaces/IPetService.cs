using PetStore.Domain.DTOs;


namespace PetStore.Domain.Interfaces;

public interface IPetService
{
    Task<PetResponse?> GetByIdAsync(int id);
    Task<PetResponse> CreateAsync(CreatePetRequest request);
    Task<PagedResult<PetResponse>> GetPagedAsync(PetQueryParameters parameters);
    Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request);
    Task DeleteAsync(int id);
}
