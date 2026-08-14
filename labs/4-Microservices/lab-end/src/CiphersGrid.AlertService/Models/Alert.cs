using CiphersGrid.SharedKernel.Enums;

namespace CiphersGrid.AlertService.Models;

public class Alert
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public required Guid RaceId { get; set; }
	public required Guid DriverId { get; set; }
	public required AlertType AlertType { get; set; }
	public required AlertSeverity Severity { get; set; }
	public required string Message { get; set; }
	public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
	public bool IsAcknowledged { get; set; } = false;
}