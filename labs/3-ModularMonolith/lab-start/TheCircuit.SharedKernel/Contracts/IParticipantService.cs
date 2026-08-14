namespace TheCircuit.SharedKernel.Contracts;

/// <summary>
/// Participants module service contract.
/// Published by Participants module; consumed by API layer and other modules.
/// Defines the public API surface for racer profile operations.
/// </summary>
public interface IParticipantService
{
    /// <summary>Gets a single racer by ID.</summary>
    Task<ParticipantDto?> GetRacerAsync(int racerId);

    /// <summary>Gets all racers registered for a specific event.</summary>
    Task<IEnumerable<ParticipantDto>> GetRegisteredRacersAsync(int eventId);

    /// <summary>Gets all registered racers (admin view).</summary>
    Task<IEnumerable<ParticipantDto>> GetAllRacersAsync();
}
