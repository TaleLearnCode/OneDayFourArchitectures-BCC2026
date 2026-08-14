using TheCircuit.Results.Data;
using TheCircuit.Results.Models;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Results.Repositories;

internal class ResultRepository
{
    private readonly ResultsDbContext _context;

    public ResultRepository(ResultsDbContext context)
    {
        _context = context;
    }

    public async Task<RaceResult?> GetByIdAsync(ResultId id)
    {
        return await _context.RaceResults.FindAsync(id.Value);
    }

    public IQueryable<RaceResult> GetAll()
    {
        return _context.RaceResults.AsQueryable();
    }

    public IQueryable<RaceResult> GetByEventId(EventId eventId)
    {
        return _context.RaceResults.Where(r => r.EventId == eventId.Value);
    }

    public async Task<RaceResult> AddAsync(RaceResult result)
    {
        _context.RaceResults.Add(result);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task UpdateAsync(RaceResult result)
    {
        _context.RaceResults.Update(result);
        await _context.SaveChangesAsync();
    }
}
