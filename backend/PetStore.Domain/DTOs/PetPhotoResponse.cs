namespace PetStore.Domain.DTOs;

public record PetPhotoResponse(
    int Id,
    int PetId,
    string Url,
    bool IsPrimary,
    int SortOrder  
);