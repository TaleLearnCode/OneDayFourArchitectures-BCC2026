using CiphersGrid.AlertService.Data;
using CiphersGrid.AlertService.Models;

namespace CiphersGrid.AlertService.Repositories;

public class AlertRepository(AlertDbContext context)
{
	public async Task<Alert?> GetByIdAsync(Guid id)
	{
		return await context.Alerts.FindAsync(id);
	}

	public async Task<IEnumerable<Alert>> GetByRaceIdAsync(Guid raceId)
	{
		return context.Alerts.Where(a => a.RaceId == raceId).OrderByDescending(a => a.IssuedAt).ToList();
	}

	public async Task<IEnumerable<Alert>> GetAllAsync()
	{
		return context.Alerts.OrderByDescending(a => a.IssuedAt).ToList();
	}

	public async Task<Alert> AddAsync(Alert alert)
	{
		await context.Alerts.AddAsync(alert);
		await context.SaveChangesAsync();
		return alert;
	}

	public async Task UpdateAsync(Alert alert)
	{
		context.Alerts.Update(alert);
		await context.SaveChangesAsync();
	}

	public async Task AcknowledgeAsync(Guid alertId)
	{
		var alert = await GetByIdAsync(alertId);
		if (alert != null)
		{
			alert.IsAcknowledged = true;
			await UpdateAsync(alert);
		}
	}
}