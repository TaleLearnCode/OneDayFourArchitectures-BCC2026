using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;

namespace TheCircuit.Api.Controllers;

/// <summary>
/// Race Events endpoints.
/// Hosted in the API layer; delegates to IEventService (Events module).
/// Follows RESTful conventions: resources as nouns, standard HTTP verbs.
/// </summary>
[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// GET /api/events
    /// Lists all race events.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    /// <summary>
    /// GET /api/events/upcoming
    /// Lists upcoming (not yet completed) race events.
    /// Teaching point: GET with filter parameter (route segment vs query string).
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcoming()
    {
        var events = await _eventService.GetUpcomingEventsAsync();
        return Ok(events);
    }

    /// <summary>
    /// GET /api/events/{eventId}
    /// Fetches a single race event by ID.
    /// Returns 404 NotFound if event does not exist.
    /// </summary>
    [HttpGet("{eventId:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(int eventId)
    {
        var eventDto = await _eventService.GetEventAsync(eventId);
        if (eventDto == null)
            return NotFound();
        return Ok(eventDto);
    }
}
