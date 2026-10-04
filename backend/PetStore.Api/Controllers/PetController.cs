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

    [HttpPut("{id}")]
    [ProducesResponseType<PetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetRequest request)
    {
        var pet = await _petService.UpdateAsync(id, request);
        return Ok(pet);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _petService.DeleteAsync(id);
        return NoContent();
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