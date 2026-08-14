using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetEvent(int eventId)
    {
        var @event = await _eventService.GetEventByIdAsync(new SharedKernel.Ids.EventId(eventId));
        return @event is null ? NotFound() : Ok(@event);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }
}
