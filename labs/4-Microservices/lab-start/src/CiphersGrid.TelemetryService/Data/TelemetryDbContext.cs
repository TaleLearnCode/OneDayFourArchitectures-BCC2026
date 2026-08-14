using CiphersGrid.TelemetryService.Models;
using Microsoft.EntityFrameworkCore;

namespace CiphersGrid.TelemetryService.Data;

public class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<LapRecord> LapRecords => Set<LapRecord>();
    public DbSet<DriverPosition> DriverPositions => Set<DriverPosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LapRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RaceId).IsRequired();
            entity.Property(e => e.DriverId).IsRequired();
            entity.HasIndex(e => e.RaceId);
            entity.HasIndex(e => e.DriverId);
        });

        modelBuilder.Entity<DriverPosition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RaceId).IsRequired();
            entity.Property(e => e.DriverId).IsRequired();
            entity.HasIndex(e => e.RaceId);
        });
    }
}
