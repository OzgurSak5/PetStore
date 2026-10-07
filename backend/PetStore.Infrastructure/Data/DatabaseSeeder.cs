using Microsoft.EntityFrameworkCore;
using PetStore.Domain.Entities;
using PetStore.Domain.Enums;

namespace PetStore.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(PetStoreDbContext context)
    {
        if(await context.Species.AnyAsync())
        {
            return;
        }

        var cat = new Species { Name = "Kedi" };
        var dog = new Species { Name = "Köpek" };

        context.Species.AddRange(cat, dog);
        await context.SaveChangesAsync();

        var britishShorthair = new Breed
        {
            Name = "British Shorthair",
            SpeciesId = cat.Id,
            EnergyLevel = 3,
            NoiseLevel = 2,
            SpaceRequirements = 2,
            AppetiteLevel = 3,
            GoodWithChildren = 4,
            GoodWithOtherPets = 3,
            GroomingNeeds = 2
        };

        var siamese = new Breed
        {
            Name = "Siyam",
            SpeciesId = cat.Id,
            EnergyLevel = 4,
            NoiseLevel = 4,
            SpaceRequirements = 3,
            AppetiteLevel = 4,
            GoodWithChildren = 3,
            GoodWithOtherPets = 2,
            GroomingNeeds = 3
        };

        var goldenRetriever = new Breed
        {
            Name = "Golden Retriever",
            SpeciesId = dog.Id,
            EnergyLevel = 5,
            NoiseLevel = 3,
            SpaceRequirements = 4,
            AppetiteLevel = 4,
            GoodWithChildren = 5,
            GoodWithOtherPets = 5,
            GroomingNeeds = 3
        };

        context.Breeds.AddRange(britishShorthair, siamese, goldenRetriever);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        var pets = new List<Pet>
        {
            new Pet
            {
                Name = "Pamuk",
                BreedId = britishShorthair.Id,
                BirthDate = new DateOnly(2022, 5, 10),
                Gender = Gender.Female,
                Price = 5500,
                Status = PetStatus.Available,
                IsVaccinated = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Pet
            {
                Name = "Milo",
                BreedId = siamese.Id,
                BirthDate = new DateOnly(2023, 2, 20),
                Gender = Gender.Male,
                Price = 6000,
                Status = PetStatus.Available,
                IsVaccinated = false,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Pet
            {
                Name = "Karabas",
                BreedId = goldenRetriever.Id,
                BirthDate = new DateOnly(2021, 8, 15),
                Gender = Gender.Male,
                Price = 8000,
                Status = PetStatus.Available,
                IsVaccinated = true,
                CreatedAt = now,
                UpdatedAt = now
            },
        };

        context.Pets.AddRange(pets);

        context.Users.Add(new User
        {
            Email = "admin@petstore.local",
            PasswordHash = "seed-placeholder",
            Role = UserRole.Admin,
            CreatedAt = now,
        });

        await context.SaveChangesAsync();
    }
}