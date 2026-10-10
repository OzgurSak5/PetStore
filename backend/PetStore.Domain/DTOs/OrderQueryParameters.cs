using System.ComponentModel.DataAnnotations;
using PetStore.Domain.Enums;

namespace PetStore.Domain.DTOs;

public class OrderQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public int? UserId { get; set; }
    public OrderStatus? Status { get; set; }
}