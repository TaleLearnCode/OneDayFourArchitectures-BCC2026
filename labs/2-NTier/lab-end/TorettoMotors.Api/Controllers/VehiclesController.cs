using Microsoft.AspNetCore.Mvc;
using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;

namespace TorettoMotors.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        var vehicles = await _vehicleService.GetAllVehiclesAsync();
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null)
            return NotFound();
        return Ok(vehicle);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetByCustomerId(int customerId)
    {
        var vehicles = await _vehicleService.GetVehiclesByCustomerIdAsync(customerId);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create(VehicleDto vehicle)
    {
        var created = await _vehicleService.CreateVehicleAsync(vehicle);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, VehicleDto vehicle)
    {
        if (id != vehicle.Id)
            return BadRequest();

        var updated = await _vehicleService.UpdateVehicleAsync(vehicle);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _vehicleService.DeleteVehicleAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }
}
