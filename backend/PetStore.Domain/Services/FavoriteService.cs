using PetStore.Domain.Entities;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;
using PetStore.Domain.Exceptions;

namespace PetStore.Domain.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPetRepository _petRepository;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IUserRepository userRepository,
        IPetRepository petRepository)
    {
        _favoriteRepository = favoriteRepository;
        _userRepository = userRepository;
        _petRepository = petRepository;
    }

    public async Task<FavoriteResponse> AddAsync(int userId, int petId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var pet = await _petRepository.GetByIdAsync(petId);

        if (pet is null)
        {
            throw new NotFoundException("Pet not found.");
        }

        var existingFavorite = await _favoriteRepository.GetAsync(userId, petId);

        if (existingFavorite is not null)
        {
            throw new ConflictException("Pet is already in favorites.");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            PetId = petId,
            Pet = pet,
            CreatedAt = DateTime.UtcNow
        };

        await _favoriteRepository.AddAsync(favorite);

        return MapToFavoriteResponse(favorite);
    }

    public async Task<PagedResult<FavoriteResponse>> GetPagedByUserAsync(int userId, FavoriteQueryParameters parameters)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var (favorites, totalCount) = await _favoriteRepository.GetPagedByUserAsync(userId, parameters);

        var items = favorites.Select(MapToFavoriteResponse).ToList();

        return new PagedResult<FavoriteResponse>(
            Items: items,
            PageNumber: parameters.PageNumber,
            PageSize: parameters.PageSize,
            TotalCount: totalCount
        );
    }

    public async Task RemoveAsync(int userId, int petId)
    {
        var favorite = await _favoriteRepository.GetAsync(userId, petId);

        if (favorite is null)
        {
            throw new NotFoundException("Favorite not found.");
        }

        await _favoriteRepository.DeleteAsync(favorite);
    }

    private static FavoriteResponse MapToFavoriteResponse(Favorite favorite)
    {
        return new FavoriteResponse(
            PetId: favorite.PetId,
            PetName: favorite.Pet?.Name ?? string.Empty,
            BreedName: favorite.Pet?.Breed?.Name ?? string.Empty,
            SpeciesName: favorite.Pet?.Breed?.Species?.Name ?? string.Empty,
            Price: favorite.Pet?.Price ?? 0,
            Status: favorite.Pet?.Status.ToString() ?? string.Empty,
            CreatedAt: favorite.CreatedAt
        );
    }
}
