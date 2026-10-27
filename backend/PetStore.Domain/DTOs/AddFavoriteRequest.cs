using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record AddFavoriteRequest(
    [Range(1, int.MaxValue)]
    int PetId
);