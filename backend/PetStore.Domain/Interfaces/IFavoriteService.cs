using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IFavoriteService
{
    Task<PagedResult<FavoriteResponse>> GetPagedByUserAsync(int userId, FavoriteQueryParameters parameters);
    Task<FavoriteResponse> AddAsync(int userId, int petId);
    Task RemoveAsync(int userId, int petId);
}
