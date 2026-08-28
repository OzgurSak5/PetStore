namespace PetStore.Domain.Entities;

public class PetPhoto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}