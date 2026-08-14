using Microsoft.AspNetCore.Mvc;
using RaceServiceClass = CiphersGrid.RaceService.Services.RaceService;
using CiphersGrid.RaceService.DTOs;

namespace CiphersGrid.RaceService.Controllers;

[ApiController]
[Route("api/races")]
public class RacesController(RaceServiceClass raceService) : ControllerBase
{
    [HttpGet("{raceId}")]
    public async Task<IActionResult> GetRace(Guid raceId)
    {
        var race = await raceService.GetRaceAsync(raceId);
        return race is null ? NotFound() : Ok(race);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRaces()
    {
        var races = await raceService.GetAllRacesAsync();
        return Ok(races);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRace([FromBody] CreateRaceRequest request)
    {
        var race = await raceService.CreateRaceAsync(request);
        return CreatedAtAction(nameof(GetRace), new { raceId = race.Id.Value }, race);
    }

    [HttpPost("{raceId}/entries")]
    public async Task<IActionResult> AddRaceEntry(Guid raceId, [FromBody] AddRaceEntryRequest request)
    {
        var entry = await raceService.AddRaceEntryAsync(raceId, request.DriverId, request.CarNumber);
        return CreatedAtAction(nameof(GetRaceEntries), new { raceId }, entry);
    }

    [HttpGet("{raceId}/entries")]
    public async Task<IActionResult> GetRaceEntries(Guid raceId)
    {
        var entries = await raceService.GetRaceEntriesAsync(raceId);
        return Ok(entries);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("Race Service is healthy");
    }
}

public record AddRaceEntryRequest(Guid DriverId, int CarNumber);
