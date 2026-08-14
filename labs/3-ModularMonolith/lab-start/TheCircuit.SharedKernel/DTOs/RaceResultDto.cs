namespace TheCircuit.SharedKernel.DTOs;

/// <summary>
/// Race result data transfer object.
/// Demonstrates cross-module data composition: Results module enriches with Participant name.
/// Returned by Results module endpoints.
/// Immutable record for API responses.
/// </summary>
public record RaceResultDto(
    int ResultId,
    int EventId,
    int RacerId,
    int FinishPosition,
    int LapTimeMs,
    int AdjustedTimeMs,
    int Points,
    string ParticipantName  // Fetched via IParticipantService.GetRacerAsync(racerId)
);
