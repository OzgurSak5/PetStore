using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;

namespace PetStore.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToResponse(user) : null;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = request.Password,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        return MapToResponse(createdUser);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role.ToString()
        );
    }
}