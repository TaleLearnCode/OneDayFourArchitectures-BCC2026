using DomsGarage.Data;
using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Services;

/// <summary>
/// Manages mechanics at Dom's Garage.
/// Simple CRUD — no scheduling, no auth, no hierarchy.
/// </summary>
public class MechanicService(GarageDbContext db)
{
    public async Task<List<Mechanic>> GetAllAsync() =>
        await db.Mechanics.Include(m => m.Jobs).ToListAsync();

    public async Task<Mechanic?> GetByIdAsync(int id) =>
        await db.Mechanics.Include(m => m.Jobs).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Mechanic> CreateAsync(Mechanic mechanic)
    {
        db.Mechanics.Add(mechanic);
        await db.SaveChangesAsync();
        return mechanic;
    }

    public async Task<Mechanic?> UpdateAsync(int id, Mechanic updated)
    {
        Mechanic? existing = await db.Mechanics.FindAsync(id);
        if (existing is null) return null;

        existing.Name = updated.Name;
        existing.Specialty = updated.Specialty;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Mechanic? mechanic = await db.Mechanics.FindAsync(id);
        if (mechanic is null) return false;

        db.Mechanics.Remove(mechanic);
        await db.SaveChangesAsync();
        return true;
    }
}
