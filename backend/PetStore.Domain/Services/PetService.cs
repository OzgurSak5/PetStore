using PetStore.Domain.Entities;
using PetStore.Domain.Enums;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;

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

    public async Task<List<PetResponse>> GetAllAsync()
    {
        var pets = await _petRepository.GetAllAsync();
        return pets.Select(MapToResponse).ToList();
    }

    public async Task<PetResponse?> GetByIdAsync(int id)
    {
        var pet = await _petRepository.GetByIdAsync(id);
        return pet is null ? null : MapToResponse(pet);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request)
    {
        var breed = await _breedRepository.GetByIdAsync(request.BreedId);

        if (breed is null)
        {
            throw new InvalidOperationException("Breed not found.");
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

        await _petRepository.AddAsync(pet);

        var createdPet = await _petRepository.AddAsync(pet);
        return MapToResponse(createdPet!);
    }
    
    private static PetResponse MapToResponse(Pet pet)
    {
        return new PetResponse(
            Id: pet.Id,
            Name: pet.Name,
            Gender: pet.Gender.ToString(),
            Status: pet.Status.ToString(),
            BreedId: pet.BreedId,
            BreedName: pet.Breed?.Name ?? string.Empty,
            SpeciesName: pet.Breed?.Species?.Name ?? string.Empty,
            BirthDate: pet.BirthDate,
            Age: CalculateAge(pet.BirthDate),
            Price: pet.Price,
            IsVaccinated: pet.IsVaccinated,
            CreatedAt: pet.CreatedAt
        );
    }

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}