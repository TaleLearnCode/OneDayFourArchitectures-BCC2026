namespace TheCircuit.SharedKernel.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new penalty.
/// Used in POST /api/penalties endpoint.
/// Participants implement the endpoint that accepts this request in the lab task.
/// </summary>
public record CreatePenaltyRequest(
    int EventId,
    int RacerId,
    string PenaltyReason,     // Must match PenaltyType enum: "Speeding", "CourseCut", "Contact", "Conduct"
    int PenaltyCostMs         // Milliseconds to add to racer's adjusted time
);
