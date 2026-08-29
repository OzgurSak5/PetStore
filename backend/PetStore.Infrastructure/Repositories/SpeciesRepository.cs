using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;

namespace PetStore.Infrastructure.Repositories;

public class SpeciesRepository : ISpeciesRepository
{
    private readonly PetStoreDbContext _context;

    public SpeciesRepository(PetStoreDbContext context)
    {
        _context = context;
    }

    public async Task<Species> AddAsync(Species species)
    {
        _context.Species.Add(species);
        await _context.SaveChangesAsync();
        return species;
    }

    public async Task<List<Species>> GetAllAsync()
    {
        return await _context.Species.ToListAsync();
    }

    public async Task<Species?> GetByIdAsync(int id)
    {
        return await _context.Species.FindAsync(id);
    }
}    