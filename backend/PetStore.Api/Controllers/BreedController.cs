using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BreedController : ControllerBase
{
    private readonly IBreedService _breedService;

    public BreedController(IBreedService breedService)
    {
        _breedService = breedService;
    }

    [HttpPut("{id}")]
    [ProducesResponseType<BreedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBreedRequest request)
    {
        var updatedBreed = await _breedService.UpdateAsync(id, request);
        return Ok(updatedBreed);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _breedService.DeleteAsync(id);
        return NoContent();
    }


    [HttpGet]
    [ProducesResponseType<PagedResult<BreedResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] BreedQueryParameters parameters)
    {
        var breeds = await _breedService.GetPagedAsync(parameters);
        return Ok(breeds);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<BreedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var breed = await _breedService.GetByIdAsync(id);
        if (breed == null)
        {
            return NotFound();
        }
        return Ok(breed);
    }

    [HttpPost]
    [ProducesResponseType<BreedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBreedRequest request)
    {
        var breed = await _breedService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = breed.Id }, breed);
    }
}