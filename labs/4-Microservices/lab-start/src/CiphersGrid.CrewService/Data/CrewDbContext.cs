using CiphersGrid.CrewService.Models;
using Microsoft.EntityFrameworkCore;

namespace CiphersGrid.CrewService.Data;

public class CrewDbContext(DbContextOptions<CrewDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<CrewMember> CrewMembers => Set<CrewMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired();
        });

        modelBuilder.Entity<CrewMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DriverId).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.ContactInfo).IsRequired();
        });
    }
}
