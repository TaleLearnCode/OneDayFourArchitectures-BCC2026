using CiphersGrid.TelemetryService.Models;
using CiphersGrid.TelemetryService.Repositories;
using CiphersGrid.SharedKernel.DTOs;

namespace CiphersGrid.TelemetryService.Services;

public class TelemetryService(
    LapRecordRepository lapRecordRepository,
    DriverPositionRepository driverPositionRepository)
{
    public async Task<LapRecordDto> RecordLapAsync(Guid raceId, Guid driverId, int lapNumber, TimeSpan lapTime)
    {
        var record = new LapRecord
        {
            RaceId = raceId,
            DriverId = driverId,
            LapNumber = lapNumber,
            LapTime = lapTime
        };

        var created = await lapRecordRepository.AddAsync(record);
        return new(new(created.Id), new(raceId), new(driverId), lapNumber, lapTime);
    }

    public async Task<IEnumerable<LapRecordDto>> GetLapsForRaceAsync(Guid raceId)
    {
        var laps = await lapRecordRepository.GetByRaceIdAsync(raceId);
        return laps.Select(l => new LapRecordDto(new(l.Id), new(l.RaceId), new(l.DriverId), l.LapNumber, l.LapTime));
    }

    public async Task<IEnumerable<DriverPositionDto>> GetRacePositionsAsync(Guid raceId)
    {
        var positions = await driverPositionRepository.GetByRaceIdAsync(raceId);
        return positions.Select(p => new DriverPositionDto(new(p.Id), new(p.RaceId), new(p.DriverId), p.Position));
    }

    public async Task UpdatePositionAsync(Guid raceId, Guid driverId, int position)
    {
        var positionRecord = await driverPositionRepository.GetOrCreateAsync(raceId, driverId);
        if (positionRecord != null)
        {
            positionRecord.Position = position;
            await driverPositionRepository.UpdateAsync(positionRecord);
        }
    }
}
