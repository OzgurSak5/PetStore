using PetStore.Domain.DTOs;
using Microsoft.AspNetCore.Http;

namespace PetStore.Domain.Interfaces;

public interface IPetPhotoService
{
    Task<List<PetPhotoResponse>> GetByPetIdAsync(int petId);
    Task<PetPhotoResponse> AddAsync(int petId, AddPetPhotoRequest request);
    Task<PetPhotoResponse> SetPrimaryAsync(int petId, int photoId);
    Task DeleteAsync(int petId, int photoId);
    Task<PetPhotoResponse> UploadAsync(int petId, IFormFile file, bool isPrimary);
}

