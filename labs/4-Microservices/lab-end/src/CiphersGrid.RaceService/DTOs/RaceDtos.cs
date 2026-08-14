namespace CiphersGrid.RaceService.DTOs;

public record CreateRaceRequest(string Name, DateTime StartTime, string TrackName);
public record AddRaceEntryRequest(Guid DriverId, int CarNumber);
