using PetStore.Domain.Enums;

namespace PetStore.Domain.Entities;


public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public PetStatus Status { get; set; }
    public int BreedId { get; set; }
    public Breed Breed { get; set; } = null!;
    public bool IsVaccinated { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateOnly BirthDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? ReservedUntil { get; set; }

    public ICollection<PetPhoto> PetPhotos { get; set; } = new List<PetPhoto>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}    