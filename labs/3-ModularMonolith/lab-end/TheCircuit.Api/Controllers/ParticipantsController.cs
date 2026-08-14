using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;

namespace TheCircuit.Api.Controllers;

/// <summary>
/// Racer (Participant) endpoints.
/// Hosted in the API layer; delegates to IParticipantService (Participants module).
/// Follows RESTful conventions: resources as nouns, standard HTTP verbs.
/// </summary>
[ApiController]
[Route("api/participants")]
public class ParticipantsController : ControllerBase
{
    private readonly IParticipantService _participantService;

    public ParticipantsController(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    /// <summary>
    /// GET /api/participants
    /// Lists all registered racers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParticipantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ParticipantDto>>> GetAll()
    {
        var racers = await _participantService.GetAllRacersAsync();
        return Ok(racers);
    }

    /// <summary>
    /// GET /api/participants/{racerId}
    /// Fetches a single racer by ID.
    /// Returns 404 NotFound if racer does not exist.
    /// </summary>
    [HttpGet("{racerId:int}")]
    [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParticipantDto>> GetById(int racerId)
    {
        var racer = await _participantService.GetRacerAsync(racerId);
        if (racer == null)
            return NotFound();
        return Ok(racer);
    }

    /// <summary>
    /// GET /api/participants/events/{eventId}
    /// Lists racers registered for a specific event.
    /// Teaching point: Cross-module query (Events concern, Participants data).
    /// </summary>
    [HttpGet("events/{eventId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ParticipantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ParticipantDto>>> GetRegisteredForEvent(int eventId)
    {
        var racers = await _participantService.GetRegisteredRacersAsync(eventId);
        return Ok(racers);
    }
}
