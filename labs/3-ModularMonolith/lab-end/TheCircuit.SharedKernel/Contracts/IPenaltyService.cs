namespace TheCircuit.SharedKernel.Contracts;

/// <summary>
/// Penalties module service contract (stub for lab task).
/// To be implemented by participants in the Penalties module.
/// Defines the public API surface for penalty operations.
/// 
/// PARTICIPANTS WILL IMPLEMENT:
/// - IssuePenaltyAsync() to create and persist a penalty
/// - Internally call IResultsService.ApplyPenaltyAsync() to update race results
/// </summary>
public interface IPenaltyService
{
    /// <summary>
    /// Issues a new penalty for a racer in a specific event.
    /// Persists the penalty to the Penalties module's database.
    /// Then calls IResultsService.ApplyPenaltyAsync() to update the racer's result.
    /// </summary>
    Task<PenaltyDto> IssuePenaltyAsync(int eventId, int racerId, string penaltyReason, int penaltyCostMs);

    /// <summary>Gets a single penalty by ID.</summary>
    Task<PenaltyDto?> GetPenaltyAsync(int penaltyId);

    /// <summary>Gets all penalties for a specific event.</summary>
    Task<IEnumerable<PenaltyDto>> GetPenaltiesForEventAsync(int eventId);

    /// <summary>Gets all penalties for a specific racer across all events.</summary>
    Task<IEnumerable<PenaltyDto>> GetPenaltiesForRacerAsync(int racerId);
}
