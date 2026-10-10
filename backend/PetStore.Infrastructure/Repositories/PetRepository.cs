using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;
using PetStore.Domain.DTOs;

namespace PetStore.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly PetStoreDbContext _context;

    public PetRepository(PetStoreDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Pet> Items, int TotalCount)> GetPagedAsync(PetQueryParameters parameters)
    {
        var query = _context.Pets.Include(p => p.Breed).ThenInclude(b => b.Species).AsQueryable();

        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        if (parameters.BreedId.HasValue)
        {
            query = query.Where(p => p.BreedId == parameters.BreedId.Value);
        }

        if (parameters.SpeciesId.HasValue)
        {
            query = query.Where(p => p.Breed.SpeciesId == parameters.SpeciesId.Value);
        }

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{parameters.Search}%"));
        }

        if (parameters.Gender.HasValue)
        {
            query = query.Where(p => p.Gender == parameters.Gender.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
        }

        if (parameters.IsVaccinated.HasValue)
        {
            query = query.Where(p => p.IsVaccinated == parameters.IsVaccinated.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync();

        return (items, totalCount);
    }
    
    public async Task<bool> ExistsByBreedIdAsync(int breedId)
    {
        return await _context.Pets.AnyAsync(p => p.BreedId == breedId);
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

    public async Task UpdateAsync(Pet pet)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();
    }
}