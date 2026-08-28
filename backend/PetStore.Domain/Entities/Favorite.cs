namespace PetStore.Domain.Entities;

public class Favorite
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}