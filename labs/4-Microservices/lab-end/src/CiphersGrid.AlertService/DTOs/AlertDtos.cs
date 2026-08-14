namespace CiphersGrid.AlertService.DTOs;

public record CreateAlertRequest(Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message);
public record AlertResponseDto(Guid Id, Guid RaceId, Guid DriverId, string AlertType, string Severity, string Message, DateTime IssuedAt, bool IsAcknowledged);