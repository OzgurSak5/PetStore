using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;

namespace PetStore.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = request.Password,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }
}