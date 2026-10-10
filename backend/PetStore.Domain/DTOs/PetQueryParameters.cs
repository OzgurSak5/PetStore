using System.ComponentModel.DataAnnotations;
using PetStore.Domain.Enums;

namespace PetStore.Domain.DTOs;

public class PetQueryParameters
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

    public PetStatus? Status { get; set; }
    public int? BreedId { get; set; }
    public int? SpeciesId { get; set; }
    public string? Search { get; set; }
    public Gender? Gender { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsVaccinated { get; set; }
}