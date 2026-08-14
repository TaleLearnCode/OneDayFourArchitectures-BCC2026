namespace CiphersGrid.TelemetryService.DTOs;

public record RecordLapRequest(Guid RaceId, Guid DriverId, int LapNumber, TimeSpan LapTime);
public record UpdatePositionRequest(int Position);
