using DomsGarage.Data;
using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Services;

/// <summary>
/// Manages service record logging for Dom's Garage.
/// Handles CRUD for ServiceRecord — follows the same pattern as JobService and CarService.
/// </summary>
public class ServiceRecordService(GarageDbContext db)
{
	public async Task<List<ServiceRecord>> GetAllAsync() =>
			await db.ServiceRecords
							.Include(sr => sr.Car)
							.Include(sr => sr.Mechanic)
							.ToListAsync();

	public async Task<ServiceRecord?> GetByIdAsync(int id) =>
			await db.ServiceRecords
							.Include(sr => sr.Car)
							.Include(sr => sr.Mechanic)
							.FirstOrDefaultAsync(sr => sr.Id == id);

	public async Task<List<ServiceRecord>> GetByCarIdAsync(int carId) =>
			await db.ServiceRecords
							.Where(sr => sr.CarId == carId)
							.Include(sr => sr.Mechanic)
							.ToListAsync();

	public async Task<ServiceRecord> CreateAsync(ServiceRecord record)
	{
		db.ServiceRecords.Add(record);
		await db.SaveChangesAsync();
		return record;
	}

	public async Task<ServiceRecord?> UpdateAsync(int id, ServiceRecord updated)
	{
		ServiceRecord? existing = await db.ServiceRecords.FindAsync(id);
		if (existing is null) return null;

		existing.CarId = updated.CarId;
		existing.MechanicId = updated.MechanicId;
		existing.ServiceDescription = updated.ServiceDescription;
		existing.DateCompleted = updated.DateCompleted;
		existing.Notes = updated.Notes;

		await db.SaveChangesAsync();
		return existing;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		ServiceRecord? record = await db.ServiceRecords.FindAsync(id);
		if (record is null) return false;

		db.ServiceRecords.Remove(record);
		await db.SaveChangesAsync();
		return true;
	}
}