using TheCircuit.Events.Models;
using TheCircuit.Events.Repositories;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Events.Services;

internal class EventService : IEventService
{
    private readonly EventRepository _repository;

    public EventService(EventRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDto?> GetEventByIdAsync(EventId eventId)
    {
        var @event = await _repository.GetByIdAsync(eventId);
        return @event is null ? null : MapToDto(@event);
    }

    public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
    {
        var events = _repository.GetAll().ToList();
        return events.Select(MapToDto);
    }

    public async Task<EventDto> CreateEventAsync(EventDto eventDto)
    {
        var @event = new Event
        {
            EventName = eventDto.EventName,
            ScheduledDate = eventDto.ScheduledDate,
            VenueId = eventDto.VenueId,
            Status = eventDto.Status
        };

        await _repository.AddAsync(@event);
        return MapToDto(@event);
    }

    private static EventDto MapToDto(Event @event)
    {
        return new EventDto(
            new EventId(@event.Id),
            @event.EventName,
            @event.ScheduledDate,
            @event.VenueId,
            @event.Status
        );
    }
}
