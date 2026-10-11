using PetStore.Domain.Entities;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IFavoriteRepository
{
    Task<Favorite?> GetAsync(int userId, int petId);
    Task AddAsync(Favorite favorite);
    Task DeleteAsync(Favorite favorite);
    Task<(List<Favorite> Items, int TotalCount)> GetPagedByUserAsync(int userId, FavoriteQueryParameters parameters);
}