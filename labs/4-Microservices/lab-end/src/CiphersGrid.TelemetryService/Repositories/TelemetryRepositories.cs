using CiphersGrid.TelemetryService.Data;
using CiphersGrid.TelemetryService.Models;

namespace CiphersGrid.TelemetryService.Repositories;

public class LapRecordRepository(TelemetryDbContext context)
{
    public async Task<LapRecord?> GetByIdAsync(Guid id)
    {
        return await context.LapRecords.FindAsync(id);
    }

    public async Task<IEnumerable<LapRecord>> GetByRaceIdAsync(Guid raceId)
    {
        return context.LapRecords.Where(l => l.RaceId == raceId).OrderBy(l => l.LapNumber).ToList();
    }

    public async Task<LapRecord> AddAsync(LapRecord record)
    {
        await context.LapRecords.AddAsync(record);
        await context.SaveChangesAsync();
        return record;
    }
}

public class DriverPositionRepository(TelemetryDbContext context)
{
    public async Task<IEnumerable<DriverPosition>> GetByRaceIdAsync(Guid raceId)
    {
        return context.DriverPositions.Where(p => p.RaceId == raceId).OrderBy(p => p.Position).ToList();
    }

    public async Task UpdateAsync(DriverPosition position)
    {
        context.DriverPositions.Update(position);
        await context.SaveChangesAsync();
    }

    public async Task<DriverPosition?> GetOrCreateAsync(Guid raceId, Guid driverId)
    {
        var existing = context.DriverPositions.FirstOrDefault(p => p.RaceId == raceId && p.DriverId == driverId);
        if (existing != null) return existing;

        var newPosition = new DriverPosition { RaceId = raceId, DriverId = driverId, Position = 0 };
        await context.DriverPositions.AddAsync(newPosition);
        await context.SaveChangesAsync();
        return newPosition;
    }
}
