namespace TheCircuit.SharedKernel.Contracts;

/// <summary>
/// Results module service contract.
/// Published by Results module; consumed by API layer and Penalties module.
/// Defines the public API surface for race result operations and penalty application.
/// 
/// CROSS-MODULE COMMUNICATION:
/// Penalties module receives IResultsService via dependency injection.
/// When a penalty is issued, Penalties calls ApplyPenaltyAsync() to update race results.
/// This demonstrates how modules collaborate through shared interfaces, not direct database access.
/// </summary>
public interface IResultsService
{
    /// <summary>Gets a single race result by ID.</summary>
    Task<RaceResultDto?> GetResultAsync(int resultId);

    /// <summary>Gets all results for a specific event (used for leaderboards).</summary>
    Task<IEnumerable<RaceResultDto>> GetResultsForEventAsync(int eventId);

    /// <summary>Gets all results for a specific racer across all events.</summary>
    Task<IEnumerable<RaceResultDto>> GetResultsForRacerAsync(int racerId);

    /// <summary>
    /// Applies a time penalty to a racer's result in a specific event.
    /// Called by Penalties module when a penalty is issued.
    /// Updates the racer's AdjustedTimeMs, which affects their final position.
    /// </summary>
    Task ApplyPenaltyAsync(int eventId, int racerId, int penaltySeconds);
}
