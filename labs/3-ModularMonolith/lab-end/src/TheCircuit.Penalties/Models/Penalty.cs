using TheCircuit.SharedKernel.Enums;

namespace TheCircuit.Penalties.Models;

internal class Penalty
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int RacerId { get; set; }
    public PenaltyReason Reason { get; set; }
    public int PenaltySeconds { get; set; }
    public DateTime IssuedAt { get; set; }
    public required string OfficialNotes { get; set; }
    public PenaltyStatus Status { get; set; }
}