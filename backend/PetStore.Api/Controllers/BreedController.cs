using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.Interfaces;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BreedController : ControllerBase
{
    private readonly IBreedService _breedService;

    public BreedController(IBreedService breedService)
    {
        _breedService = breedService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var breeds = await _breedService.GetAllAsync();
        return Ok(breeds);
    }

    [HttpGet("{id}")]
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
    public async Task<IActionResult> Create([FromBody] CreateBreedRequest request)
    {
        var breed = await _breedService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = breed.Id }, breed);
    }
}