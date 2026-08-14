using Microsoft.AspNetCore.Mvc;
using TelemetryServiceClass = CiphersGrid.TelemetryService.Services.TelemetryService;

namespace CiphersGrid.TelemetryService.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController(TelemetryServiceClass telemetryService) : ControllerBase
{
    [HttpPost("laps")]
    public async Task<IActionResult> RecordLap([FromBody] RecordLapRequest request)
    {
        var lap = await telemetryService.RecordLapAsync(request.RaceId, request.DriverId, request.LapNumber, request.LapTime);
        return CreatedAtAction(nameof(GetLapsForRace), new { raceId = request.RaceId }, lap);
    }

    [HttpGet("races/{raceId}/laps")]
    public async Task<IActionResult> GetLapsForRace(Guid raceId)
    {
        var laps = await telemetryService.GetLapsForRaceAsync(raceId);
        return Ok(laps);
    }

    [HttpGet("races/{raceId}/positions")]
    public async Task<IActionResult> GetRacePositions(Guid raceId)
    {
        var positions = await telemetryService.GetRacePositionsAsync(raceId);
        return Ok(positions);
    }

    [HttpPut("races/{raceId}/positions/{driverId}")]
    public async Task<IActionResult> UpdatePosition(Guid raceId, Guid driverId, [FromBody] UpdatePositionRequest request)
    {
        await telemetryService.UpdatePositionAsync(raceId, driverId, request.Position);
        return NoContent();
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Telemetry Service is healthy");
    }
}

public record RecordLapRequest(Guid RaceId, Guid DriverId, int LapNumber, TimeSpan LapTime);
public record UpdatePositionRequest(int Position);
