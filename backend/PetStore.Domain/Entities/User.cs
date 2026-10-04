using PetStore.Domain.Enums;

namespace PetStore.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}