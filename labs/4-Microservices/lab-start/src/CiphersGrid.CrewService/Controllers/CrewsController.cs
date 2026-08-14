using Microsoft.AspNetCore.Mvc;
using CrewServiceClass = CiphersGrid.CrewService.Services.CrewService;
using CiphersGrid.CrewService.DTOs;

namespace CiphersGrid.CrewService.Controllers;

[ApiController]
[Route("api/crews")]
public class CrewsController(CrewServiceClass crewService) : ControllerBase
{
    [HttpGet("drivers/{driverId}")]
    public async Task<IActionResult> GetDriver(Guid driverId)
    {
        var driver = await crewService.GetDriverAsync(driverId);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpGet("drivers")]
    public async Task<IActionResult> GetAllDrivers()
    {
        var drivers = await crewService.GetAllDriversAsync();
        return Ok(drivers);
    }

    [HttpPost("drivers")]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverRequest request)
    {
        var driver = await crewService.CreateDriverAsync(request);
        return CreatedAtAction(nameof(GetDriver), new { driverId = driver.Id.Value }, driver);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Crew Service is healthy");
    }
}
