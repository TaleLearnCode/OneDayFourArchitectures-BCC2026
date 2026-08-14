using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.Contracts;

public interface IEventService
{
    Task<EventDto?> GetEventByIdAsync(EventId eventId);
    Task<IEnumerable<EventDto>> GetAllEventsAsync();
    Task<EventDto> CreateEventAsync(EventDto eventDto);
}
