using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PetStore.Infrastructure.Data;

public class PetStoreDbContextFactory : IDesignTimeDbContextFactory<PetStoreDbContext>
{
    public PetStoreDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets("1af4f9f8-5d05-4be3-bffc-319cba513b9d")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<PetStoreDbContext>();
        
        optionsBuilder.UseNpgsql(connectionString);

        return new PetStoreDbContext(optionsBuilder.Options);
    }
}