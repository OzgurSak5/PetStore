using PetStore.Domain.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.Interfaces;

public interface IPetPhotoService
{
    Task<List<PetPhotoResponse>> GetByPetIdAsync(int petId);
    Task<PetPhotoResponse> AddAsync(int petId, AddPetPhotoRequest request);
    Task<PetPhotoResponse> SetPrimaryAsync(int petId, int photoId);
    Task DeleteAsync(int petId, int photoId);
}

public record AddPetPhotoRequest(
    [Required]
    [Url]
    [StringLength(500)]
    string Url,
    bool IsPrimary
);