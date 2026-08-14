using CiphersGrid.RaceService.Models;
using CiphersGrid.RaceService.Repositories;
using CiphersGrid.RaceService.Clients;
using CiphersGrid.SharedKernel.DTOs;
using CiphersGrid.RaceService.DTOs;

namespace CiphersGrid.RaceService.Services;

public class RaceService(
    RaceRepository raceRepository,
    RaceEntryRepository raceEntryRepository,
    AlertServiceClient alertServiceClient)
{
    public async Task<RaceDto?> GetRaceAsync(Guid raceId)
    {
        var race = await raceRepository.GetByIdAsync(raceId);
        return race is null ? null : MapToDto(race);
    }

    public async Task<IEnumerable<RaceDto>> GetAllRacesAsync()
    {
        var races = await raceRepository.GetAllAsync();
        return races.Select(MapToDto);
    }

    public async Task<RaceDto> CreateRaceAsync(CreateRaceRequest request)
    {
        var race = new Race
        {
            Name = request.Name,
            StartTime = request.StartTime,
            TrackName = request.TrackName
        };

        var created = await raceRepository.AddAsync(race);
        return MapToDto(created);
    }

    public async Task<RaceEntryDto> AddRaceEntryAsync(Guid raceId, Guid driverId, int carNumber)
    {
        var entry = new RaceEntry
        {
            RaceId = raceId,
            DriverId = driverId,
            CarNumber = carNumber
        };

        var created = await raceEntryRepository.AddAsync(entry);
        
        // Notify Alert Service when entry is created
        try
        {
            await alertServiceClient.CreateAlertAsync(new(
                raceId,
                driverId,
                "Broadcast",
                "Low",
                $"Driver {driverId} registered for race {raceId}"
            ));
        }
        catch
        {
            // Graceful degradation if Alert Service is unavailable
        }

        return new(new(raceId), new(driverId), carNumber);
    }

    public async Task<IEnumerable<RaceEntryDto>> GetRaceEntriesAsync(Guid raceId)
    {
        var entries = await raceEntryRepository.GetByRaceIdAsync(raceId);
        return entries.Select(e => new RaceEntryDto(new(e.RaceId), new(e.DriverId), e.CarNumber));
    }

    private static RaceDto MapToDto(Race race)
    {
        return new RaceDto(
            new(race.Id),
            race.Name,
            race.StartTime,
            race.TrackName
        );
    }
}
