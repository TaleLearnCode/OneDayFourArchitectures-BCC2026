using Microsoft.EntityFrameworkCore;
using TorettoMotors.DAL.Entities;

namespace TorettoMotors.DAL.Context;

public class TorettoDbContext : DbContext
{
    public TorettoDbContext(DbContextOptions<TorettoDbContext> options)
        : base(options)
    {
    }

    public required DbSet<CustomerEntity> Customers { get; set; }
    public required DbSet<VehicleEntity> Vehicles { get; set; }
    public required DbSet<PartEntity> Parts { get; set; }
    public required DbSet<InvoiceEntity> Invoices { get; set; }
    public required DbSet<MaintenancePlanEntity> MaintenancePlans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer configuration
        modelBuilder.Entity<CustomerEntity>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<CustomerEntity>()
            .HasMany(c => c.Vehicles)
            .WithOne(v => v.Customer)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerEntity>()
            .HasMany(c => c.Invoices)
            .WithOne(i => i.Customer)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Vehicle configuration
        modelBuilder.Entity<VehicleEntity>()
            .HasKey(v => v.Id);

        // Part configuration
        modelBuilder.Entity<PartEntity>()
            .HasKey(p => p.Id);

        // Invoice configuration
        modelBuilder.Entity<InvoiceEntity>()
            .HasKey(i => i.Id);

        // MaintenancePlan configuration
        modelBuilder.Entity<MaintenancePlanEntity>()
            .HasKey(m => m.Id);

        // Seed data
        modelBuilder.Entity<CustomerEntity>().HasData(
            new CustomerEntity
            {
                Id = 1,
                Name = "Dominic Toretto",
                Email = "dom@torettomotors.com",
                Phone = "555-0101",
                DateCreated = new DateTime(2024, 1, 15)
            },
            new CustomerEntity
            {
                Id = 2,
                Name = "Letty Ortiz",
                Email = "letty@torettomotors.com",
                Phone = "555-0102",
                DateCreated = new DateTime(2024, 2, 10)
            }
        );

        modelBuilder.Entity<VehicleEntity>().HasData(
            new VehicleEntity
            {
                Id = 1,
                CustomerId = 1,
                Make = "Nissan",
                Model = "350Z",
                Year = 2006,
                LicensePlate = "TORETTO1",
                Mileage = 45000
            },
            new VehicleEntity
            {
                Id = 2,
                CustomerId = 1,
                Make = "Dodge",
                Model = "Charger",
                Year = 1970,
                LicensePlate = "TORETTO2",
                Mileage = 92000
            }
        );
    }
}
