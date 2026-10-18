using PetStore.Domain.DTOs;
using PetStore.Domain.Interfaces;
using PetStore.Domain.Entities;
using PetStore.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace PetStore.Domain.Services;

public class PetPhotoService : IPetPhotoService
{
    private readonly IPetPhotoRepository _petPhotoRepository;
    private readonly IPetRepository _petRepository;
    private readonly IFileStorageService _fileStorageService;

    public PetPhotoService(IPetPhotoRepository petPhotoRepository, IPetRepository petRepository, IFileStorageService fileStorageService)
    {
        _petPhotoRepository = petPhotoRepository;
        _petRepository = petRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<PetPhotoResponse> UploadAsync(int petId, IFormFile file, bool isPrimary)
    {
        var pet = await _petRepository.GetByIdAsync(petId);

        if (pet == null)
        {
            throw new NotFoundException("Pet not found.");
        }

        var url = await _fileStorageService.SaveAsync(file);

        var petPhotos = await _petPhotoRepository.GetByPetIdAsync(petId);

        bool shouldBePrimary = isPrimary || petPhotos.Count == 0;

        if (shouldBePrimary)
        {
            await _petPhotoRepository.ClearPrimaryAsync(petId);
        }

        var order = await _petPhotoRepository.GetMaxSortOrderAsync(petId);

        var petPhoto = new PetPhoto
        {
            PetId = petId,
            Url = url,
            IsPrimary = shouldBePrimary,
            SortOrder = order + 1
        };

        await _petPhotoRepository.AddAsync(petPhoto);
        return MapToResponse(petPhoto);
    }

    public async Task<PetPhotoResponse> AddAsync(int petId, AddPetPhotoRequest request)
    {
        var pet = await _petRepository.GetByIdAsync(petId);

        if (pet == null)
        {
            throw new NotFoundException("Pet not found.");
        }

        var petPhotos = await _petPhotoRepository.GetByPetIdAsync(petId);

        bool shouldBePrimary = request.IsPrimary || petPhotos.Count == 0;

        if (shouldBePrimary)
        {
            await _petPhotoRepository.ClearPrimaryAsync(petId);
        }

        var order = await _petPhotoRepository.GetMaxSortOrderAsync(petId);

        var petPhoto = new PetPhoto
        {
            PetId = petId,
            Url = request.Url,
            IsPrimary = shouldBePrimary,
            SortOrder = order + 1
        };

        await _petPhotoRepository.AddAsync(petPhoto);
        return MapToResponse(petPhoto);
    }

    public async Task DeleteAsync(int petId, int photoId)
    {
        var petPhoto = await _petPhotoRepository.GetByIdAsync(photoId);

        if (petPhoto == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        if (petPhoto.PetId != petId)
        {
            throw new NotFoundException("Photo not found.");
        }

        var wasPrimary = petPhoto.IsPrimary;
        var url = petPhoto.Url;

        await _petPhotoRepository.DeleteAsync(petPhoto);

        _fileStorageService.Delete(url);

        if (wasPrimary)
        {
            var remainingPhotos = await _petPhotoRepository.GetByPetIdAsync(petId);

            if (remainingPhotos.Count > 0)
            {
                var newPrimary = remainingPhotos[0];
                newPrimary.IsPrimary = true;
                await _petPhotoRepository.UpdateAsync(newPrimary);
            }
        }
    }

    public async Task<List<PetPhotoResponse>> GetByPetIdAsync(int petId)
    {
        var pet = await _petRepository.GetByIdAsync(petId);
        if (pet == null)
        {
            throw new NotFoundException("Pet not found.");
        }

        var petPhotos = await _petPhotoRepository.GetByPetIdAsync(petId);

        return petPhotos.Select(MapToResponse).ToList();
    }

    public async Task<PetPhotoResponse> SetPrimaryAsync(int petId, int photoId)
    {
        var petPhoto = await _petPhotoRepository.GetByIdAsync(photoId);

        if (petPhoto == null)
        {
            throw new NotFoundException("Photo not found.");
        }

        if (petPhoto.PetId != petId)
        {
            throw new NotFoundException("Photo not found.");
        }

        await _petPhotoRepository.ClearPrimaryAsync(petId);
        petPhoto.IsPrimary = true;
        await _petPhotoRepository.UpdateAsync(petPhoto);
        return MapToResponse(petPhoto);
    }

    private static PetPhotoResponse MapToResponse(PetPhoto photo)
    {
        return new PetPhotoResponse(
            Id: photo.Id,
            PetId: photo.PetId,
            Url: photo.Url,
            IsPrimary: photo.IsPrimary,
            SortOrder: photo.SortOrder
        );
    }
}