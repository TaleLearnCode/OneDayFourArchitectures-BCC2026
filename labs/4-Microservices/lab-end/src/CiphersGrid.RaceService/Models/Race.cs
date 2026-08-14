using CiphersGrid.SharedKernel.Ids;

namespace CiphersGrid.RaceService.Models;

public class Race
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public DateTime StartTime { get; set; }
    public required string TrackName { get; set; }
}

public class RaceEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid RaceId { get; set; }
    public required Guid DriverId { get; set; }
    public int CarNumber { get; set; }
}
