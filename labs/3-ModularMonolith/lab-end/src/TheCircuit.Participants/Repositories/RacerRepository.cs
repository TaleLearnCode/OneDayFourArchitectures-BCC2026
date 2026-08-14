using TheCircuit.Participants.Data;
using TheCircuit.Participants.Models;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Participants.Repositories;

internal class RacerRepository
{
    private readonly ParticipantsDbContext _context;

    public RacerRepository(ParticipantsDbContext context)
    {
        _context = context;
    }

    public async Task<Racer?> GetByIdAsync(RacerId id)
    {
        return await _context.Racers.FindAsync(id.Value);
    }

    public IQueryable<Racer> GetAll()
    {
        return _context.Racers.AsQueryable();
    }

    public async Task<Racer> AddAsync(Racer racer)
    {
        _context.Racers.Add(racer);
        await _context.SaveChangesAsync();
        return racer;
    }

    public async Task UpdateAsync(Racer racer)
    {
        _context.Racers.Update(racer);
        await _context.SaveChangesAsync();
    }
}
