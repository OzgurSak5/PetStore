namespace PetStore.Domain.Entities;

public class Breed
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SpeciesId { get; set; }
    public Species Species { get; set; } = null!;
    public int EnergyLevel { get; set; }
    public int NoiseLevel { get; set; }
    public int SpaceRequirements { get; set; }
    public int AppetiteLevel { get; set; }
    public int GoodWithChildren { get; set; }
    public int GoodWithOtherPets { get; set; }
    public int GroomingNeeds { get; set; }


    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}