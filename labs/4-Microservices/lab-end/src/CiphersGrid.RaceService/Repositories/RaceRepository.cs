using CiphersGrid.RaceService.Data;
using CiphersGrid.RaceService.Models;

namespace CiphersGrid.RaceService.Repositories;

public class RaceRepository(RaceDbContext context)
{
    public async Task<Race?> GetByIdAsync(Guid id)
    {
        return await context.Races.FindAsync(id);
    }

    public async Task<IEnumerable<Race>> GetAllAsync()
    {
        return context.Races.ToList();
    }

    public async Task<Race> AddAsync(Race race)
    {
        await context.Races.AddAsync(race);
        await context.SaveChangesAsync();
        return race;
    }

    public async Task UpdateAsync(Race race)
    {
        context.Races.Update(race);
        await context.SaveChangesAsync();
    }
}

public class RaceEntryRepository(RaceDbContext context)
{
    public async Task<RaceEntry?> GetByIdAsync(Guid id)
    {
        return await context.RaceEntries.FindAsync(id);
    }

    public async Task<IEnumerable<RaceEntry>> GetByRaceIdAsync(Guid raceId)
    {
        return context.RaceEntries.Where(e => e.RaceId == raceId).ToList();
    }

    public async Task<RaceEntry> AddAsync(RaceEntry entry)
    {
        await context.RaceEntries.AddAsync(entry);
        await context.SaveChangesAsync();
        return entry;
    }
}
