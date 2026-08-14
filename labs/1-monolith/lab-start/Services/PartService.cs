using DomsGarage.Data;
using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Services;

/// <summary>
/// Manages parts inventory at Dom's Garage.
/// Standalone CRUD — no job-to-parts link in the scaffold (no JobPart join entity).
/// Mechanics browse inventory manually via GET /api/parts.
/// </summary>
public class PartService(GarageDbContext db)
{
    public async Task<List<Part>> GetAllAsync() =>
        await db.Parts.ToListAsync();

    public async Task<Part?> GetByIdAsync(int id) =>
        await db.Parts.FindAsync(id);

    public async Task<Part> CreateAsync(Part part)
    {
        db.Parts.Add(part);
        await db.SaveChangesAsync();
        return part;
    }

    public async Task<Part?> UpdateAsync(int id, Part updated)
    {
        Part? existing = await db.Parts.FindAsync(id);
        if (existing is null) return null;

        existing.Name = updated.Name;
        existing.StockQuantity = updated.StockQuantity;
        existing.UnitCost = updated.UnitCost;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Part? part = await db.Parts.FindAsync(id);
        if (part is null) return false;

        db.Parts.Remove(part);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Adjusts stock level for a part — used when a mechanic pulls a part for a job.
    /// </summary>
    public async Task<Part?> AdjustStockAsync(int id, int quantityDelta)
    {
        Part? part = await db.Parts.FindAsync(id);
        if (part is null) return null;

        part.StockQuantity += quantityDelta;
        if (part.StockQuantity < 0)
            throw new InvalidOperationException($"Insufficient stock for part '{part.Name}'.");

        await db.SaveChangesAsync();
        return part;
    }
}
