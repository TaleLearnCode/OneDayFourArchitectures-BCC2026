using DomsGarage.Models;
using Microsoft.EntityFrameworkCore;

namespace DomsGarage.Data;

/// <summary>
/// The single shared database context for all of Dom's Garage.
///
/// WALKTHROUGH STOP — Anti-Pattern 1: The Omniscient DbContext
/// This one class knows about every entity in the system. At four entities it's readable.
/// At 40 entities, this file becomes a change hotspot — every feature addition touches it.
/// That's your first merge conflict waiting to happen.
///
/// Anti-pattern note: no schema isolation, no per-domain contexts.
/// One brain. Knows everything.
/// </summary>
public class GarageDbContext(DbContextOptions<GarageDbContext> options) : DbContext(options)
{
	public DbSet<Car> Cars => Set<Car>();
	public DbSet<Mechanic> Mechanics => Set<Mechanic>();
	public DbSet<Job> Jobs => Set<Job>();
	public DbSet<Part> Parts => Set<Part>();
	public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Car>(entity =>
		{
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Make).IsRequired().HasMaxLength(50);
			entity.Property(c => c.Model).IsRequired().HasMaxLength(50);
			entity.Property(c => c.LicensePlate).IsRequired().HasMaxLength(20);
			entity.Property(c => c.Status).HasConversion<string>();
		});

		modelBuilder.Entity<Mechanic>(entity =>
		{
			entity.HasKey(m => m.Id);
			entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
			entity.Property(m => m.Specialty).IsRequired().HasMaxLength(100);
		});

		modelBuilder.Entity<Job>(entity =>
		{
			entity.HasKey(j => j.Id);
			entity.Property(j => j.Description).IsRequired().HasMaxLength(500);
			entity.Ignore(j => j.IsComplete);  // Derived property — not persisted
			entity.HasOne(j => j.Car)
								.WithMany(c => c.Jobs)
								.HasForeignKey(j => j.CarId);
			entity.HasOne(j => j.Mechanic)
								.WithMany(m => m.Jobs)
								.HasForeignKey(j => j.MechanicId);
		});

		modelBuilder.Entity<Part>(entity =>
		{
			entity.HasKey(p => p.Id);
			entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
			entity.Property(p => p.UnitCost).HasColumnType("decimal(10,2)");
		});

		modelBuilder.Entity<ServiceRecord>(entity =>
		{
			entity.HasKey(sr => sr.Id);
			entity.Property(sr => sr.ServiceDescription).IsRequired().HasMaxLength(500);
			entity.Property(sr => sr.Notes).HasMaxLength(1000);
			entity.HasOne(sr => sr.Car)
						.WithMany(c => c.ServiceRecords)
						.HasForeignKey(sr => sr.CarId);
			entity.HasOne(sr => sr.Mechanic)
						.WithMany(m => m.ServiceRecords)
						.HasForeignKey(sr => sr.MechanicId);
		});
	}
}
