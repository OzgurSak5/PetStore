using PetStore.Domain.DTOs;
using System.ComponentModel.DataAnnotations;
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

public record AddPetPhotoRequest(
    [Required]
    [Url]
    [StringLength(500)]
    string Url,
    bool IsPrimary
);

public class UploadPetPhotoRequest
{
    public IFormFile File { get; set; } = null!;
    public bool IsPrimary { get; set; }
}