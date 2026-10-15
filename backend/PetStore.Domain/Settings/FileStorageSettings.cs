namespace PetStore.Domain.Settings;

public class FileStorageSettings
{
    public string UploadPath { get; set; } = "uploads/pets";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };

    public string BaseUrl { get; set; } = "http://localhost:5087";
}