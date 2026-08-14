using TheCircuit.Events.Data;
using TheCircuit.Events.Models;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Events.Repositories;

internal class EventRepository
{
    private readonly EventsDbContext _context;

    public EventRepository(EventsDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(EventId id)
    {
        return await _context.Events.FindAsync(id.Value);
    }

    public IQueryable<Event> GetAll()
    {
        return _context.Events.AsQueryable();
    }

    public async Task<Event> AddAsync(Event @event)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();
        return @event;
    }

    public async Task UpdateAsync(Event @event)
    {
        _context.Events.Update(@event);
        await _context.SaveChangesAsync();
    }
}
