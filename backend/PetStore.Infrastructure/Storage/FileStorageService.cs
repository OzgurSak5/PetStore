using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PetStore.Domain.Exceptions;
using PetStore.Domain.Interfaces;
using PetStore.Domain.Settings;

namespace PetStore.Infrastructure.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IOptions<FileStorageSettings> settings, IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _environment = environment;
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        // 1. Boş dosya kontrolü
        // file null mu, Length sıfır mı → ValidationException
        if(file == null || file.Length == 0)
        {
            throw new ValidationException("File is empty");
        }

        // 2. Boyut kontrolü
        // file.Length > _settings.MaxFileSizeBytes → ValidationException

        if(file.Length > _settings.MaxFileSizeBytes)
        {
            var maxMb = _settings.MaxFileSizeBytes / 1024 / 1024;
            throw new ValidationException($"File size cannot exceed {maxMb} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
 
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            var allowed = string.Join(", ", _settings.AllowedExtensions);
            throw new ValidationException($"Only these file types are allowed: {allowed}");
        }
 
        using var stream = file.OpenReadStream();
 
        var header = new byte[4];
        await stream.ReadExactlyAsync(header, 0, 4);
 
        if (!IsValidImage(header))
        {
            throw new ValidationException("File is not a valid image.");
        }
 
        var uploadRoot = Path.Combine(_environment.WebRootPath, _settings.UploadPath);
 
        if (!Directory.Exists(uploadRoot))
        {
            Directory.CreateDirectory(uploadRoot);
        }
 
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadRoot, fileName);
 
        stream.Position = 0;
 
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream);
        }
 
        return $"{_settings.BaseUrl}/{_settings.UploadPath}/{fileName}";
    }

    public void Delete(string url)
    {
        try
        {
            var fileName = Path.GetFileName(url);
            var fullPath = Path.Combine(_environment.WebRootPath, _settings.UploadPath, fileName);
 
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Dosya silinemezse veritabanı işlemi devam etsin.
        }
    }

    private static bool IsValidImage(byte[] header)
    {
        // JPEG
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return true;
        }
 
        // PNG
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
        {
            return true;
        }
 
        // WebP (RIFF)
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
        {
            return true;
        }
 
        return false;
    }
}