using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.DTOs;

public record RaceResultDto(
    ResultId Id,
    EventId EventId,
    RacerId RacerId,
    int FinishPosition,
    long LapTimeMs,
    long AdjustedTimeMs,
    int Points,
    RaceResultStatus Status
);
