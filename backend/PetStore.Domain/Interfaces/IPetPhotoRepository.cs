using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IPetPhotoRepository
{
    Task<List<PetPhoto>> GetByPetIdAsync(int PetId);
    Task<PetPhoto?> GetByIdAsync(int Id);
    Task <PetPhoto> AddAsync(PetPhoto petPhoto);
    Task UpdateAsync(PetPhoto petPhoto);
    Task DeleteAsync(PetPhoto petPhoto);

    Task<int> GetMaxSortOrderAsync(int PetId);
    Task ClearPrimaryAsync(int PetId);
}

