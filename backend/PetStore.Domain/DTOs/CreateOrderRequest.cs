using System.ComponentModel.DataAnnotations;

namespace PetStore.Domain.DTOs;

public record CreateOrderRequest(
    [Required]
    [MinLength(1)]
    List<int> PetIds
);