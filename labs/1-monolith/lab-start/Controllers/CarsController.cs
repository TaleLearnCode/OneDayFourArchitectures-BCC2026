using DomsGarage.Models;
using DomsGarage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomsGarage.Controllers;

/// <summary>
/// REST endpoints for car management.
/// CRUD + the FlagReadyForPickup business rule endpoint.
/// </summary>
[ApiController]
[Route("api/cars")]
public class CarsController(CarService carService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Car>>> GetAll() =>
        Ok(await carService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Car>> GetById(int id)
    {
        Car? car = await carService.GetByIdAsync(id);
        return car is null ? NotFound() : Ok(car);
    }

    [HttpPost]
    public async Task<ActionResult<Car>> Create(Car car)
    {
        Car created = await carService.CreateAsync(car);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Car>> Update(int id, Car car)
    {
        Car? updated = await carService.UpdateAsync(id, car);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await carService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// WALKTHROUGH STOP — business rule endpoint.
    /// Validates all jobs are closed, then sets Car.Status = ReadyForPickup.
    /// One DbContext. One method call. No network hops. That's the monolith pattern.
    /// </summary>
    [HttpPatch("{id:int}/ready")]
    public async Task<ActionResult<Car>> FlagReady(int id)
    {
        try
        {
            Car? car = await carService.FlagReadyForPickupAsync(id);
            return car is null ? NotFound() : Ok(car);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
