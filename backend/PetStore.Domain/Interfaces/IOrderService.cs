using System.ComponentModel.DataAnnotations;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderResponse>> GetPagedAsync(OrderQueryParameters parameters);
    Task<OrderResponse?> GetByIdAsync(int id, CurrentUser currentUser);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CurrentUser currentUser);
    Task<OrderResponse> PayAsync(int id, CurrentUser currentUser);
    Task<OrderResponse> CancelAsync(int id, CurrentUser currentUser);
    Task CancelExpiredReservationsAsync();
}

public record CreateOrderRequest(
    [Required]
    [MinLength(1)]
    List<int> PetIds
);