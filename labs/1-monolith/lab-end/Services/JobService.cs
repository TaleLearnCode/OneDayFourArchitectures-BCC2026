using DomsGarage.Data;
using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Services;

/// <summary>
/// Manages repair jobs at Dom's Garage.
///
/// Business rule: CloseJobAsync checks whether all jobs for a car are now closed.
/// If yes, it automatically sets the car's status to ReadyForPickup.
/// This rule lives here — not in a domain event, not in a policy object.
/// One service. One DbContext. In-process. That's the monolith pattern.
/// </summary>
public class JobService(GarageDbContext db)
{
    public async Task<List<Job>> GetAllAsync() =>
        await db.Jobs.Include(j => j.Car).Include(j => j.Mechanic).ToListAsync();

    public async Task<Job?> GetByIdAsync(int id) =>
        await db.Jobs.Include(j => j.Car).Include(j => j.Mechanic).FirstOrDefaultAsync(j => j.Id == id);

    public async Task<List<Job>> GetByCarIdAsync(int carId) =>
        await db.Jobs.Where(j => j.CarId == carId)
                     .Include(j => j.Mechanic)
                     .ToListAsync();

    public async Task<Job> CreateAsync(Job job)
    {
        // Opening a new job moves the car to InProgress
        Car? car = await db.Cars.FindAsync(job.CarId);
        if (car is not null && car.Status == CarStatus.InGarage)
            car.Status = CarStatus.InProgress;

        job.OpenedAt = DateTime.UtcNow;
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    /// <summary>
    /// Closes an open job and auto-updates the car's status if all jobs are done.
    /// This is the monolith's business rule in plain view — no indirection needed.
    /// </summary>
    public async Task<Job?> CloseJobAsync(int id)
    {
        Job? job = await db.Jobs.Include(j => j.Car)
                                 .ThenInclude(c => c!.Jobs)
                                 .FirstOrDefaultAsync(j => j.Id == id);
        if (job is null) return null;
        if (job.ClosedAt.HasValue) return job;  // Already closed

        job.ClosedAt = DateTime.UtcNow;

        // Auto-status: if all jobs for this car are now closed, mark car ready
        // Car is always loaded here via Include/ThenInclude above
        if (job.Car is not null)
        {
            bool allClosed = job.Car.Jobs.All(j => j.Id == id || j.ClosedAt.HasValue);
            if (allClosed)
                job.Car.Status = CarStatus.ReadyForPickup;
        }

        await db.SaveChangesAsync();
        return job;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Job? job = await db.Jobs.FindAsync(id);
        if (job is null) return false;

        db.Jobs.Remove(job);
        await db.SaveChangesAsync();
        return true;
    }
}
