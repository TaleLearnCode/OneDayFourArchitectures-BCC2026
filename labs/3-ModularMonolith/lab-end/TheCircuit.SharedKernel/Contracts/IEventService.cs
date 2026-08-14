namespace TheCircuit.SharedKernel.Contracts;

/// <summary>
/// Events module service contract.
/// Published by Events module; consumed by API layer.
/// Defines the public API surface for race event operations.
/// </summary>
public interface IEventService
{
    /// <summary>Gets a single race event by ID.</summary>
    Task<EventDto?> GetEventAsync(int eventId);

    /// <summary>Gets all upcoming (non-completed) race events.</summary>
    Task<IEnumerable<EventDto>> GetUpcomingEventsAsync();

    /// <summary>Gets all race events regardless of status.</summary>
    Task<IEnumerable<EventDto>> GetAllEventsAsync();
}
