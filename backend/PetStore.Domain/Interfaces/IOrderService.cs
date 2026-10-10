using System.ComponentModel.DataAnnotations;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderResponse>> GetPagedAsync(OrderQueryParameters parameters);
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request);
    Task<OrderResponse> PayAsync(int id);
    Task<OrderResponse> CancelAsync(int id);
}

public record CreateOrderRequest(
    [Range(1, int.MaxValue)]
    int UserId,

    [Required]
    [MinLength(1)]
    List<int> PetIds
);