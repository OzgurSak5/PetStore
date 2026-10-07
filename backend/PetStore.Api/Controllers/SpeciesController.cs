using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SpeciesController : ControllerBase
{
    private readonly ISpeciesService _speciesService;

    public SpeciesController(ISpeciesService speciesService)
    {
        _speciesService = speciesService;
    }

    [HttpPut("{id}")]
    [ProducesResponseType<SpeciesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSpeciesRequest request)
    {
        var species = await _speciesService.UpdateAsync(id, request);
        return Ok(species);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _speciesService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType<List<SpeciesResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var speciesList = await _speciesService.GetAllAsync();
        return Ok(speciesList);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<SpeciesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var species = await _speciesService.GetByIdAsync(id);
        if (species == null)
        {
            return NotFound();
        }
        return Ok(species);
    }

    [HttpPost]
    [ProducesResponseType<SpeciesResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSpeciesRequest request)
    {
        var species = await _speciesService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = species.Id }, species);
    }
}