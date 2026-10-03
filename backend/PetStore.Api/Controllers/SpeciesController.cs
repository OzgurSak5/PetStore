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
    public async Task<IActionResult> Create([FromBody] string name)
    {
        var species = await _speciesService.CreateAsync(name);
        return CreatedAtAction(nameof(GetById), new { id = species.Id }, species);
    }
}