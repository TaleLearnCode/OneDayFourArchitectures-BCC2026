namespace DomsGarage.Models;

/// <summary>
/// Core entity — the vehicle Dom's Garage is tracking.
/// Status is updated automatically by JobService when all jobs are closed.
/// Anti-pattern note: Status couples CarService and JobService through shared state.
/// </summary>
public class Car
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public CarStatus Status { get; set; } = CarStatus.InGarage;

    // Navigation properties for EF Core
    public ICollection<Job> Jobs { get; set; } = [];

    // LAB Step 1: Add navigation property for ServiceRecords here
    // public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];
}
