using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Infrastructure.Data;
using PetStore.Domain.DTOs;

namespace PetStore.Infrastructure.Repositories;

public class BreedRepository : IBreedRepository
{
    private readonly PetStoreDbContext _context;

    public BreedRepository(PetStoreDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Breed> Items, int TotalCount)> GetPagedAsync(BreedQueryParameters parameters)
    {
        var query = _context.Breeds.Include(b => b.Species).AsQueryable();

        if (parameters.SpeciesId.HasValue)
        {
            query = query.Where(b => b.SpeciesId == parameters.SpeciesId.Value);
        }

        if (parameters.MaxEnergyLevel.HasValue)
        {
            query = query.Where(b => b.EnergyLevel <= parameters.MaxEnergyLevel.Value);
        }

        if (parameters.MaxNoiseLevel.HasValue)
        {
            query = query.Where(b => b.NoiseLevel <= parameters.MaxNoiseLevel.Value);
        }

        if (parameters.MaxSpaceRequirements.HasValue)
        {
            query = query.Where(b => b.SpaceRequirements <= parameters.MaxSpaceRequirements.Value);
        }

        if (parameters.MinGoodWithChildren.HasValue)
        {
            query = query.Where(b => b.GoodWithChildren >= parameters.MinGoodWithChildren.Value);
        }

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(b => EF.Functions.ILike(b.Name, $"%{parameters.Search}%"));
        }

        var totalCount = await query.CountAsync();

        var items = await query.OrderBy(b => b.Name)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return (items, totalCount);
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

    public async Task<Breed?> GetByIdAsync(int id)
    {
        return await _context.Breeds.Include(b => b.Species).FirstOrDefaultAsync(b => b.Id == id);
    }
}