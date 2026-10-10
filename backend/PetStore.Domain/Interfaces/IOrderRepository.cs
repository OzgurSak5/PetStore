using PetStore.Domain.DTOs;
using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<(List<Order> Items, int TotalCount)> GetPagedAsync(OrderQueryParameters parameters);
}