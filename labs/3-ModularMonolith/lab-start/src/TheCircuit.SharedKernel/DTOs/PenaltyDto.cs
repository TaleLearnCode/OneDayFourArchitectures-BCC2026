using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.DTOs;

public record PenaltyDto(
    PenaltyId Id,
    EventId EventId,
    RacerId RacerId,
    PenaltyReason Reason,
    int PenaltySeconds,
    DateTime IssuedAt,
    string OfficialNotes,
    PenaltyStatus Status
);
