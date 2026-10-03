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

    [HttpGet]
    [ProducesResponseType<List<BreedResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var breeds = await _breedService.GetAllAsync();
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