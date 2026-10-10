namespace PetStore.Domain.DTOs;

public record OrderResponse(
    int Id,
    int UserId,
    string UserEmail,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    int Id,
    int PetId,
    string PetName,
    string BreedName,
    decimal Price
);