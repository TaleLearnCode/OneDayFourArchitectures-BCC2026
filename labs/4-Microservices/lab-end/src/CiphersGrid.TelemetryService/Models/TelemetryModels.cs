namespace CiphersGrid.TelemetryService.Models;

public class LapRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid RaceId { get; set; }
    public required Guid DriverId { get; set; }
    public int LapNumber { get; set; }
    public TimeSpan LapTime { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class DriverPosition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid RaceId { get; set; }
    public required Guid DriverId { get; set; }
    public int Position { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
