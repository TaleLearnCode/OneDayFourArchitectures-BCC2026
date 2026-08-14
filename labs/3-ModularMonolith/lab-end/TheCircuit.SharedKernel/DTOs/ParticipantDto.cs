namespace TheCircuit.SharedKernel.DTOs;

/// <summary>
/// Racer (participant) data transfer object.
/// Returned by Participants module endpoints.
/// Immutable record for API responses.
/// </summary>
public record ParticipantDto(
    int RacerId,
    string FullName,
    string LicenseNumber,
    string TeamName,
    bool IsActive
);
