using CiphersGrid.RaceService.Models;
using Microsoft.EntityFrameworkCore;

namespace CiphersGrid.RaceService.Data;

public class RaceDbContext(DbContextOptions<RaceDbContext> options) : DbContext(options)
{
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RaceEntry> RaceEntries => Set<RaceEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Race>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.TrackName).IsRequired();
        });

        modelBuilder.Entity<RaceEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RaceId).IsRequired();
            entity.Property(e => e.DriverId).IsRequired();
        });
    }
}
