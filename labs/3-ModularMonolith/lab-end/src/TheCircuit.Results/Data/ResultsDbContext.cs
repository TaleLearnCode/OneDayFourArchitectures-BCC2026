using Microsoft.EntityFrameworkCore;
using TheCircuit.Results.Models;

namespace TheCircuit.Results.Data;

internal class ResultsDbContext : DbContext
{
    public ResultsDbContext(DbContextOptions<ResultsDbContext> options)
        : base(options)
    {
    }

    public DbSet<RaceResult> RaceResults => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RaceResult>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.EventId).IsRequired();
            entity.Property(r => r.RacerId).IsRequired();
            entity.Property(r => r.Status).HasConversion<int>();
        });
    }
}
