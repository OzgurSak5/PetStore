namespace PetStore.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public decimal Price { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
}