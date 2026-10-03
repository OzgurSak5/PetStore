using Microsoft.AspNetCore.Mvc;
using PetStore.Domain.Interfaces;

namespace PetStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetController : ControllerBase
{
    private readonly IPetService _petService;

    public PetController(IPetService petService)
    {
        _petService = petService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pets = await _petService.GetAllAsync();
        return Ok(pets);
    }

    [HttpGet("{id}")]
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
    public async Task<IActionResult> Create([FromBody] CreatePetRequest request)
    {
        var pet = await _petService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }
}