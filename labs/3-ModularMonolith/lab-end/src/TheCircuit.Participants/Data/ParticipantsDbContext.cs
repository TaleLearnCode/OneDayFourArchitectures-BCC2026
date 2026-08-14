using Microsoft.EntityFrameworkCore;
using TheCircuit.Participants.Models;

namespace TheCircuit.Participants.Data;

internal class ParticipantsDbContext : DbContext
{
    public ParticipantsDbContext(DbContextOptions<ParticipantsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Racer> Racers => Set<Racer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Racer>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.FullName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.LicenseNumber).IsRequired().HasMaxLength(50);
            entity.Property(r => r.TeamName).IsRequired().HasMaxLength(100);
        });
    }
}
