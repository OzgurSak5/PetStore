using Microsoft.AspNetCore.Http;

namespace PetStore.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file);
    void Delete(string url);
}