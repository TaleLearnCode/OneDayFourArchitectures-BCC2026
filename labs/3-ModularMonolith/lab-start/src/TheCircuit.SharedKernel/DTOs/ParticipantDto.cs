using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.SharedKernel.DTOs;

public record ParticipantDto(
    RacerId Id,
    string FullName,
    string LicenseNumber,
    string TeamName,
    bool IsActive
);
