using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.DTOs;
using PetStore.Domain.Interfaces;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/User/{userId}/favorites")]
[Produces("application/json")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<FavoriteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(int userId, [FromQuery] FavoriteQueryParameters parameters)
    {
        var favorites = await _favoriteService.GetPagedByUserAsync(userId, parameters);
        return Ok(favorites);
    }

    [HttpPost]
    [ProducesResponseType<FavoriteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(int userId, [FromBody] AddFavoriteRequest request)
    {
        var favorite = await _favoriteService.AddAsync(userId, request.PetId);
        return CreatedAtAction(nameof(GetAll), new { userId }, favorite);
    }


    [HttpDelete("{petId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int userId, int petId)
    {
        await _favoriteService.RemoveAsync(userId, petId);
        return NoContent();
    }
}