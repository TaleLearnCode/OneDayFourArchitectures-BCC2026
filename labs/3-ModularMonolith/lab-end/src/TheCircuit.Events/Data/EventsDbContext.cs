using Microsoft.EntityFrameworkCore;
using TheCircuit.Events.Models;

namespace TheCircuit.Events.Data;

internal class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.VenueId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<int>();
        });
    }
}
