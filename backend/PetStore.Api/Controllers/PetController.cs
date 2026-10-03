using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.Interfaces;
using PetStore.Domain.DTOs;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PetController : ControllerBase
{
    private readonly IPetService _petService;

    public PetController(IPetService petService)
    {
        _petService = petService;
    }

    [HttpGet]
    [ProducesResponseType<List<PetResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var pets = await _petService.GetAllAsync();
        return Ok(pets);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var pet = await _petService.GetByIdAsync(id);
        if (pet == null)
        {
            return NotFound();
        }
        return Ok(pet);

    }

    [HttpPost]
    [ProducesResponseType<PetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePetRequest request)
    {
        var pet = await _petService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    
}