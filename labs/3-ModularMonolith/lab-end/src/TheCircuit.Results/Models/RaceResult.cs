using TheCircuit.SharedKernel.Enums;

namespace TheCircuit.Results.Models;

internal class RaceResult
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int RacerId { get; set; }
    public int FinishPosition { get; set; }
    public long LapTimeMs { get; set; }
    public long AdjustedTimeMs { get; set; }
    public int Points { get; set; }
    public RaceResultStatus Status { get; set; }
}
