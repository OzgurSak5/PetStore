using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record AddPetPhotoRequest(
    [Required]
    [Url]
    [StringLength(500)]
    string Url,
    bool IsPrimary
);