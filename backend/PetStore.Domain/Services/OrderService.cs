using Microsoft.Extensions.Options;
using PetStore.Domain.DTOs;
using PetStore.Domain.Entities;
using PetStore.Domain.Enums;
using PetStore.Domain.Exceptions;
using PetStore.Domain.Interfaces;
using PetStore.Domain.Settings;

namespace PetStore.Domain.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPetRepository _petRepository;
    private readonly OrderSettings _orderSettings;

    public OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IPetRepository petRepository,
        IOptions<OrderSettings> orderSettings)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _petRepository = petRepository;
        _orderSettings = orderSettings.Value;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
        {
            throw new ValidationException("User not found.");
        }

        var petIds = request.PetIds.Distinct().ToList();

        if (petIds.Count != request.PetIds.Count)
        {
            throw new ValidationException("Duplicate pet IDs are not allowed.");
        }

        var pets = new List<Pet>();

        foreach (var petId in petIds)
        {
            var pet = await _petRepository.GetByIdAsync(petId);

            if (pet == null)
            {
                throw new ValidationException($"Pet with ID {petId} not found.");
            }

            if (pet.Status != PetStatus.Available)
            {
                throw new ValidationException($"Pet with ID {petId} is not available for order.");
            }

            pets.Add(pet);
        }

        var now = DateTime.UtcNow;
        var reservedUntil = now.AddMinutes(_orderSettings.ReservationMinutes);

        var order = new Order
        {
            UserId = request.UserId,
            Status = OrderStatus.Created,
            TotalPrice = pets.Sum(p => p.Price),
            CreatedAt = now
        };


        foreach (var pet in pets)
        {
            order.OrderItems.Add(new OrderItem
            {
                PetId = pet.Id,
                Price = pet.Price
            });

            pet.Status = PetStatus.Pending;
            pet.ReservedUntil = reservedUntil;
            pet.UpdatedAt = now;
        }

        await _orderRepository.AddAsync(order);
        var createdOrder = await _orderRepository.GetByIdAsync(order.Id);
        return MapToResponse(createdOrder!);
    }


    public async Task<OrderResponse> PayAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        if (order.Status != OrderStatus.Created)
        {
            throw new ConflictException($"Order is already {order.Status}.");
        }

        order.Status = OrderStatus.Paid;

        foreach (var item in order.OrderItems)
        {
            item.Pet.Status = PetStatus.Sold;
            item.Pet.ReservedUntil = null;
            item.Pet.UpdatedAt = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    public async Task<OrderResponse> CancelAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        if (order.Status != OrderStatus.Created)
        {
            throw new ConflictException($"Only orders in Created status can be cancelled. Current status: {order.Status}.");
        }

        order.Status = OrderStatus.Cancelled;

        foreach (var item in order.OrderItems)
        {
            item.Pet.Status = PetStatus.Available;
            item.Pet.ReservedUntil = null;
            item.Pet.UpdatedAt = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order != null ? MapToResponse(order) : null;
    }

    public async Task<PagedResult<OrderResponse>> GetPagedAsync(OrderQueryParameters parameters)
    {
        var (orders, totalCount) = await _orderRepository.GetPagedAsync(parameters);

        var items = orders.Select(MapToResponse).ToList();

        return new PagedResult<OrderResponse>(
            Items: items,
            PageNumber: parameters.PageNumber,
            PageSize: parameters.PageSize,
            TotalCount: totalCount
        );
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse(
            Id: order.Id,
            UserId: order.UserId,
            UserEmail: order.User?.Email ?? string.Empty,
            Status: order.Status.ToString(),
            TotalPrice: order.TotalPrice,
            CreatedAt: order.CreatedAt,
            Items: order.OrderItems.Select(oi => new OrderItemResponse(
                Id: oi.Id,
                PetId: oi.PetId,
                PetName: oi.Pet?.Name ?? string.Empty,
                BreedName: oi.Pet?.Breed?.Name ?? string.Empty,
                Price: oi.Price
            )).ToList()
        );
    }
}