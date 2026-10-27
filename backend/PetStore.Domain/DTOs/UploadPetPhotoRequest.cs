using Microsoft.AspNetCore.Http;

namespace PetStore.Domain.DTOs;

public class UploadPetPhotoRequest
{
    public IFormFile File { get; set; } = null!;
    public bool IsPrimary { get; set; }
}