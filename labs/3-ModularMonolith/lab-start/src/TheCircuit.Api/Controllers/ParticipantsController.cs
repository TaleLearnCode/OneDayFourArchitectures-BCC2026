using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly IParticipantService _participantService;

    public ParticipantsController(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    [HttpGet("{racerId}")]
    public async Task<IActionResult> GetRacer(int racerId)
    {
        var racer = await _participantService.GetRacerByIdAsync(new RacerId(racerId));
        return racer is null ? NotFound() : Ok(racer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRacers()
    {
        var racers = await _participantService.GetAllRacersAsync();
        return Ok(racers);
    }
}
