using PetStore.Domain.Entities;
using PetStore.Domain.Enums;
using PetStore.Domain.Interfaces;

namespace PetStore.Domain.Services;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IBreedRepository _breedRepository;

    public PetService(IPetRepository petRepository, IBreedRepository breedRepository)
    {
        _petRepository = petRepository;
        _breedRepository = breedRepository;
    }

    public async Task<List<Pet>> GetAllAsync()
    {
        return await _petRepository.GetAllAsync();
    }

    public async Task<Pet?> GetByIdAsync(int id)
    {
        return await _petRepository.GetByIdAsync(id);
    }

    public async Task<Pet> CreateAsync(CreatePetRequest request)
    {
        var breed = await _breedRepository.GetByIdAsync(request.BreedId);

        if (breed is null)
        {
            throw new InvalidOperationException("Belirtilen cins bulunamadı.");
        }

        var pet = new Pet
        {
            Name = request.Name,
            BreedId = request.BreedId,
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            Price = request.Price,
            IsVaccinated = request.IsVaccinated,
            Status = PetStatus.Available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _petRepository.AddAsync(pet);
    }
}