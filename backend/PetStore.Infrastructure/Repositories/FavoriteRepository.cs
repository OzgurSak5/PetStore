using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.DTOs;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;

namespace PetStore.Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly PetStoreDbContext _dbContext;

    public FavoriteRepository(PetStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Favorite?> GetAsync(int userId, int petId)
    {
        return await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PetId == petId);
    }

    public async Task AddAsync(Favorite favorite)
    {
        await _dbContext.Favorites.AddAsync(favorite);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Favorite favorite)
    {
        _dbContext.Favorites.Remove(favorite);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(List<Favorite> Items, int TotalCount)> GetPagedByUserAsync(int userId, FavoriteQueryParameters parameters)
    {
        var query = _dbContext.Favorites
            .Include(f => f.Pet)
                .ThenInclude(p => p.Breed)
                    .ThenInclude(b => b.Species)
            .Where(f => f.UserId == userId && !f.Pet.IsDeleted)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}