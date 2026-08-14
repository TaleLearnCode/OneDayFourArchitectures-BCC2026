using Microsoft.AspNetCore.Mvc;
using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;

namespace TorettoMotors.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly IPartService _partService;

    public PartsController(IPartService partService)
    {
        _partService = partService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetAll()
    {
        var parts = await _partService.GetAllPartsAsync();
        return Ok(parts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PartDto>> GetById(int id)
    {
        var part = await _partService.GetPartByIdAsync(id);
        if (part == null)
            return NotFound();
        return Ok(part);
    }

    [HttpPost]
    public async Task<ActionResult<PartDto>> Create(PartDto part)
    {
        var created = await _partService.CreatePartAsync(part);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PartDto part)
    {
        if (id != part.Id)
            return BadRequest();

        var updated = await _partService.UpdatePartAsync(part);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _partService.DeletePartAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }
}
