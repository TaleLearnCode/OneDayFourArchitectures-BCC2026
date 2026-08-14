using DomsGarage.Models;
using DomsGarage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomsGarage.Controllers;

/// <summary>
/// REST endpoints for parts inventory management.
/// </summary>
[ApiController]
[Route("api/parts")]
public class PartsController(PartService partService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Part>>> GetAll() =>
        Ok(await partService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Part>> GetById(int id)
    {
        Part? part = await partService.GetByIdAsync(id);
        return part is null ? NotFound() : Ok(part);
    }

    [HttpPost]
    public async Task<ActionResult<Part>> Create(Part part)
    {
        Part created = await partService.CreateAsync(part);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Part>> Update(int id, Part part)
    {
        Part? updated = await partService.UpdateAsync(id, part);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await partService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Adjusts stock level. Positive = restock, negative = parts pulled for a job.
    /// </summary>
    [HttpPatch("{id:int}/stock")]
    public async Task<ActionResult<Part>> AdjustStock(int id, [FromQuery] int delta)
    {
        try
        {
            Part? part = await partService.AdjustStockAsync(id, delta);
            return part is null ? NotFound() : Ok(part);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
