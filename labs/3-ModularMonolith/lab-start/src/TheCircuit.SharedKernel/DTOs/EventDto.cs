using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.DTOs;

public record EventDto(
    EventId Id,
    string EventName,
    DateTime ScheduledDate,
    string VenueId,
    EventStatus Status
);
