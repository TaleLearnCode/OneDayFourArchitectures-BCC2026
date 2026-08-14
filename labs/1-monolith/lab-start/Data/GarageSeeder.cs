using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Data;

/// <summary>
/// Seeds Dom's Garage with F&amp;F-themed demo data.
/// Cars: iconic Fast &amp; Furious vehicles.
/// Mechanics: cast members from the crew.
/// Parts: shop inventory that makes the place feel real.
///
/// Walkthrough note: seed data adds personality without affecting architecture.
/// Participants can ignore it entirely and still understand the pattern.
/// </summary>
public static class GarageSeeder
{
    public static async Task SeedAsync(GarageDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Cars.AnyAsync()) return;  // Already seeded

        // --- Mechanics ---
        Mechanic domToretto = new()
        {
            Name = "Dominic Toretto",
            Specialty = "Engine Overhaul"
        };
        Mechanic hanSeoulOh = new()
        {
            Name = "Han Seoul-Oh",
            Specialty = "Suspension & Tuning"
        };

        db.Mechanics.AddRange(domToretto, hanSeoulOh);
        await db.SaveChangesAsync();

        // --- Cars ---
        Car charger = new()
        {
            Make = "Dodge",
            Model = "Charger R/T",
            Year = 1970,
            LicensePlate = "DOM-1970",
            Status = CarStatus.InProgress
        };
        Car supra = new()
        {
            Make = "Toyota",
            Model = "Supra MK4",
            Year = 1995,
            LicensePlate = "BRIAN-95",
            Status = CarStatus.InGarage
        };

        db.Cars.AddRange(charger, supra);
        await db.SaveChangesAsync();

        // --- Jobs ---
        Job engineRebuild = new()
        {
            CarId = charger.Id,
            MechanicId = domToretto.Id,
            Description = "Full engine rebuild — 900hp build for the quarter mile",
            OpenedAt = DateTime.UtcNow.AddDays(-3)
        };

        db.Jobs.Add(engineRebuild);
        await db.SaveChangesAsync();

        // --- Parts ---
        Part nitrousOxide = new()
        {
            Name = "Nitrous Oxide Canister (NOS)",
            StockQuantity = 12,
            UnitCost = 149.99m
        };

        db.Parts.Add(nitrousOxide);
        await db.SaveChangesAsync();
    }
}
