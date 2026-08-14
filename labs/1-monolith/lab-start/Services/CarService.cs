using DomsGarage.Data;
using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Services;

/// <summary>
/// Manages car check-in/check-out and status transitions.
///
/// WALKTHROUGH STOP — Monolith Superpower: no network hops.
/// FlagReadyForPickup queries Jobs and updates Car in one method, one DbContext,
/// zero network calls. One method. In-process. Instant.
///
/// Anti-pattern note (3a): No isolation — a null-ref here crashes all features.
/// Anti-pattern note (3b): All-or-nothing deploy — a fix here ships everything.
/// </summary>
public class CarService(GarageDbContext db)
{
    public async Task<List<Car>> GetAllAsync() =>
        await db.Cars.Include(c => c.Jobs).ToListAsync();

    public async Task<Car?> GetByIdAsync(int id) =>
        await db.Cars.Include(c => c.Jobs).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Car> CreateAsync(Car car)
    {
        db.Cars.Add(car);
        await db.SaveChangesAsync();
        return car;
    }

    public async Task<Car?> UpdateAsync(int id, Car updated)
    {
        Car? existing = await db.Cars.FindAsync(id);
        if (existing is null) return null;

        existing.Make = updated.Make;
        existing.Model = updated.Model;
        existing.Year = updated.Year;
        existing.LicensePlate = updated.LicensePlate;
        existing.Status = updated.Status;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Car? car = await db.Cars.FindAsync(id);
        if (car is null) return false;

        db.Cars.Remove(car);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// WALKTHROUGH MOMENT — the monolith's key business rule in 8 lines.
    /// One DbContext. One method. No HTTP calls. No queues. No retries.
    /// Query jobs → validate all closed → update status. That's it.
    /// </summary>
    public async Task<Car?> FlagReadyForPickupAsync(int carId)
    {
        Car? car = await db.Cars.Include(c => c.Jobs).FirstOrDefaultAsync(c => c.Id == carId);
        if (car is null) return null;

        bool allJobsClosed = car.Jobs.All(j => j.ClosedAt.HasValue);
        if (!allJobsClosed)
            throw new InvalidOperationException("Cannot mark car ready — one or more jobs are still open.");

        car.Status = CarStatus.ReadyForPickup;
        await db.SaveChangesAsync();
        return car;
    }
}
