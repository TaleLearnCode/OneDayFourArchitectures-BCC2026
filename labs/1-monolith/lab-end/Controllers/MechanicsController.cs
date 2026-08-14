using DomsGarage.Models;
using DomsGarage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomsGarage.Controllers;

/// <summary>
/// REST endpoints for mechanic management.
/// </summary>
[ApiController]
[Route("api/mechanics")]
public class MechanicsController(MechanicService mechanicService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Mechanic>>> GetAll() =>
        Ok(await mechanicService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Mechanic>> GetById(int id)
    {
        Mechanic? mechanic = await mechanicService.GetByIdAsync(id);
        return mechanic is null ? NotFound() : Ok(mechanic);
    }

    [HttpPost]
    public async Task<ActionResult<Mechanic>> Create(Mechanic mechanic)
    {
        Mechanic created = await mechanicService.CreateAsync(mechanic);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Mechanic>> Update(int id, Mechanic mechanic)
    {
        Mechanic? updated = await mechanicService.UpdateAsync(id, mechanic);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await mechanicService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
