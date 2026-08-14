using TheCircuit.Penalties.Data;
using TheCircuit.Penalties.Models;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Penalties.Repositories;

internal class PenaltyRepository
{
	private readonly PenaltiesDbContext _context;

	public PenaltyRepository(PenaltiesDbContext context)
	{
		_context = context;
	}

	public async Task<Penalty?> GetByIdAsync(PenaltyId id)
	{
		return await _context.Penalties.FindAsync(id.Value);
	}

	public IQueryable<Penalty> GetAll()
	{
		return _context.Penalties.AsQueryable();
	}

	public IQueryable<Penalty> GetByEventId(EventId eventId)
	{
		return _context.Penalties.Where(p => p.EventId == eventId.Value);
	}

	public async Task<Penalty> AddAsync(Penalty penalty)
	{
		_context.Penalties.Add(penalty);
		await _context.SaveChangesAsync();
		return penalty;
	}

	public async Task UpdateAsync(Penalty penalty)
	{
		_context.Penalties.Update(penalty);
		await _context.SaveChangesAsync();
	}
}