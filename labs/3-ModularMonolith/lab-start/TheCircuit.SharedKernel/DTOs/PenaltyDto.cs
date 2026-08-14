namespace TheCircuit.SharedKernel.DTOs;

/// <summary>
/// Race penalty data transfer object.
/// Returned by Penalties module endpoints.
/// Immutable record for API responses.
/// </summary>
public record PenaltyDto(
    int PenaltyId,
    int EventId,
    int RacerId,
    string PenaltyReason,        // Enum string: "Speeding", "CourseCut", "Contact", "Conduct"
    int PenaltyCostMs,           // Milliseconds added to adjusted time
    DateTime AppliedDate,
    string Status                // "Issued", "Appealed", "Dismissed"
);
