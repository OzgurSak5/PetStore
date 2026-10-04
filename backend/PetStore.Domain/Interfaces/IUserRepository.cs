using PetStore.Domain.Entities;

namespace PetStore.Domain.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> AddAsync(User user);

    Task UpdateAsync(User user);
}