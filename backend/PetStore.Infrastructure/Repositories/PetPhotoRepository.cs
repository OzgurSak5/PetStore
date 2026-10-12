using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.DTOs;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;

namespace PetStore.Infrastructure.Repositories;

public class PetPhotoRepository : IPetPhotoRepository
{
    private readonly PetStoreDbContext _dbContext;

    public PetPhotoRepository(PetStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PetPhoto> AddAsync(PetPhoto petPhoto)
    {
        _dbContext.PetPhotos.Add(petPhoto);
        await _dbContext.SaveChangesAsync();
        return petPhoto;
    }

    public async Task ClearPrimaryAsync(int PetId)
    {
        var photos = await _dbContext.PetPhotos.Where(p => p.PetId == PetId && p.IsPrimary).ToListAsync();

        foreach(var photo in photos)
        {
            photo.IsPrimary = false;
        }
    }

    public async Task DeleteAsync(PetPhoto petPhoto)
    {
        _dbContext.PetPhotos.Remove(petPhoto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PetPhoto?> GetByIdAsync(int id)
    {
        return await _dbContext.PetPhotos.FindAsync(id);
    }

    public async Task<List<PetPhoto>> GetByPetIdAsync(int petId)
    {
        return await _dbContext.PetPhotos.Where(p => p.PetId == petId).OrderBy(p => p.SortOrder).ToListAsync();
    }

    public async Task<int> GetMaxSortOrderAsync(int PetId)
    {
        var photos = await _dbContext.PetPhotos
        .Where(p => p.PetId == PetId).Select(p => p.SortOrder).ToListAsync();

        return photos.Count == 0 ? 0 : photos.Max(); 
    }

    public async Task UpdateAsync(PetPhoto petPhoto)
    {
        _dbContext.PetPhotos.Update(petPhoto);
        await _dbContext.SaveChangesAsync();
    }
}