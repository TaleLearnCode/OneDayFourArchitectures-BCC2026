using CiphersGrid.SharedKernel.Ids;

namespace CiphersGrid.SharedKernel.DTOs;

public record DriverDto(DriverId Id, string FirstName, string LastName, string LicenseNumber, bool IsActive);
public record CrewMemberDto(CrewMemberId Id, DriverId DriverId, string Role, string ContactInfo);
public record RaceDto(RaceId Id, string Name, DateTime StartTime, string TrackName);
public record RaceEntryDto(RaceId RaceId, DriverId DriverId, int CarNumber);
public record LapRecordDto(LapRecordId Id, RaceId RaceId, DriverId DriverId, int LapNumber, TimeSpan LapTime);
public record DriverPositionDto(DriverPositionId Id, RaceId RaceId, DriverId DriverId, int Position);
public record CreateAlertRequestDto(Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message);
public record AlertResponseDto(Guid Id, Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message, DateTime IssuedAt, bool IsAcknowledged);
