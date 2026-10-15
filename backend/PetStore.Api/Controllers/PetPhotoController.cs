using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.DTOs;
using PetStore.Domain.Interfaces;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/Pet/{petId}/photos")]
[Produces("application/json")]
public class PetPhotoController : ControllerBase
{
    private readonly IPetPhotoService _petPhotoService;

    public PetPhotoController(IPetPhotoService petPhotoService)
    {
        _petPhotoService = petPhotoService;
    }

    [HttpGet]
    [ProducesResponseType<List<PetPhotoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(int petId)
    {
        var petPhoto = await _petPhotoService.GetByPetIdAsync(petId);
        return Ok(petPhoto);
    }

    [HttpPost]
    [ProducesResponseType<PetPhotoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add(int petId, [FromBody] AddPetPhotoRequest request)
    {
        var petPhoto = await _petPhotoService.AddAsync(petId, request);
        return CreatedAtAction(nameof(GetAll), new { petId }, petPhoto);
    }

    [HttpPut("{photoId}/primary")]
    [ProducesResponseType<PetPhotoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimary(int petId, int photoId)
    {
        var petPhoto = await _petPhotoService.SetPrimaryAsync(petId, photoId);
        return Ok(petPhoto);
    }

    [HttpDelete("{photoId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int petId, int photoId)
    {
        await _petPhotoService.DeleteAsync(petId, photoId);
        return NoContent();
    }

}