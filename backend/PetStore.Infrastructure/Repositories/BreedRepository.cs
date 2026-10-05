using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;

namespace PetStore.Infrastructure.Repositories;

public class BreedRepository : IBreedRepository
{
    private readonly PetStoreDbContext _context;

    public BreedRepository(PetStoreDbContext context)
    {
        _context = context;
    }

    public async Task UpdateAsync(Breed breed)
    {
        _context.Breeds.Update(breed);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Breed breed)
    {
        _context.Breeds.Remove(breed);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsBySpeciesIdAsync(int speciesId)
    {
        return await _context.Breeds.AnyAsync(b => b.SpeciesId == speciesId);
    }

    public async Task<Breed> AddAsync(Breed breed)
    {
        _context.Breeds.Add(breed);
        await _context.SaveChangesAsync();
        return breed;
    }

    public async Task<List<Breed>> GetAllAsync()
    {
        return await _context.Breeds.Include(b => b.Species).ToListAsync();
    }

    public async Task<Breed?> GetByIdAsync(int id)
    {
        return await _context.Breeds.Include(b => b.Species).FirstOrDefaultAsync(b => b.Id == id);
    }
}