using PetStore.Domain.Entities;
using PetStore.Domain.Enums;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;
using PetStore.Domain.Exceptions;

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

    public async Task<PagedResult<PetResponse>> GetPagedAsync(PetQueryParameters parameters)
    {
        var (pets, totalCount) = await _petRepository.GetPagedAsync(parameters);

        var items = pets.Select(MapToResponse).ToList();

        return new PagedResult<PetResponse>(
            Items: items,
            PageNumber: parameters.PageNumber,
            PageSize: parameters.PageSize,
            TotalCount: totalCount
        );
    }

    public async Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request)
    {
        var pet = await _petRepository.GetByIdAsync(id);

        if (pet is null)
        {
            throw new NotFoundException("Pet not found.");
        }

        var breed = await _breedRepository.GetByIdAsync(request.BreedId);

        if (breed is null)
        {
            throw new ValidationException("Breed not found.");
        }

        if (request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ValidationException("Birth date cannot be in the future.");
        }

        pet.Name = request.Name;
        pet.BreedId = request.BreedId;
        pet.BirthDate = request.BirthDate;
        pet.Gender = request.Gender;
        pet.Price = request.Price;
        pet.IsVaccinated = request.IsVaccinated;
        pet.Status = request.Status;
        pet.UpdatedAt = DateTime.UtcNow;

        await _petRepository.UpdateAsync(pet);
        var updatedPet = await _petRepository.GetByIdAsync(id);
        return MapToResponse(updatedPet!);
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await _petRepository.GetByIdAsync(id);

        if (pet is null)
        {
            throw new NotFoundException("Pet not found.");
        }

        pet.IsDeleted = true;
        pet.UpdatedAt = DateTime.UtcNow;

        await _petRepository.UpdateAsync(pet);
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
            throw new ValidationException("Breed not found.");
        }

        if (request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ValidationException("Birth date cannot be in the future.");
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

        var createdPet = await _petRepository.GetByIdAsync(pet.Id);
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