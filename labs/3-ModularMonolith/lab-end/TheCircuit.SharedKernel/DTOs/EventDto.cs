namespace TheCircuit.SharedKernel.DTOs;

/// <summary>
/// Race event data transfer object.
/// Returned by Events module endpoints.
/// Immutable record for API responses.
/// </summary>
public record EventDto(
    int EventId,
    string EventName,
    DateTime ScheduledDate,
    int VenueId,
    string Status  // "Scheduled", "InProgress", "Completed", "Cancelled"
);
