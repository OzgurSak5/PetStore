using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.Interfaces;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly ISpeciesService _speciesService;

    public SpeciesController(ISpeciesService speciesService)
    {
        _speciesService = speciesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var speciesList = await _speciesService.GetAllAsync();
        return Ok(speciesList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var species = await _speciesService.GetByIdAsync(id);
        if (species == null)
        {
            return NotFound();
        }
        return Ok(species);
    }
}