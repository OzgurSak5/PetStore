using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.DTOs;
using PetStore.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PetStore.Api.Extensions;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/User/{userId}/favorites")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
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
        if (!CanAccess(userId))
        {
            return Forbid();
        }

        var favorites = await _favoriteService.GetPagedByUserAsync(userId, parameters);
        return Ok(favorites);
    }

    [HttpPost]
    [ProducesResponseType<FavoriteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(int userId, [FromBody] AddFavoriteRequest request)
    {
        if (!CanAccess(userId))
        {
            return Forbid();
        }

        var favorite = await _favoriteService.AddAsync(userId, request.PetId);
        return CreatedAtAction(nameof(GetAll), new { userId }, favorite);
    }


    [HttpDelete("{petId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int userId, int petId)
    {
        if (!CanAccess(userId))
        {
            return Forbid();
        }

        await _favoriteService.RemoveAsync(userId, petId);
        return NoContent();
    }

    private bool CanAccess(int userId) => userId == User.GetUserId() || User.IsAdmin();
}