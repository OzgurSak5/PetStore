using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;
using PetStore.Domain.Exceptions;

namespace PetStore.Domain.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var email = EmailNormalizer.Normalize(request.Email);

        if (email != user.Email && await _userRepository.EmailExistsAsync(email))
        {
            throw new ConflictException("Email already exists.");
        }

        user.Email = email;
        user.Role = request.Role;

        await _userRepository.UpdateAsync(user);

        return MapToResponse(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.IsDeleted = true;

        await _userRepository.UpdateAsync(user);
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
        var email = EmailNormalizer.Normalize(request.Email);

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new ConflictException("Email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
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