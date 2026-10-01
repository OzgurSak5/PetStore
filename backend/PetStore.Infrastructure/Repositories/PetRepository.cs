using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;

namespace PetStore.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly PetStoreDbContext _context;

    public PetRepository(PetStoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pet>> GetAllAsync()
    {
        return await _context.Pets.Include(p => p.Breed).ThenInclude(b => b.Species).ToListAsync();
    }

    public async Task<Pet?> GetByIdAsync(int id)
    {
        return await _context.Pets.Include(p => p.Breed).ThenInclude(b => b.Species).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Pet> AddAsync(Pet pet)
    {
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        return pet;
    }
}