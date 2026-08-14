namespace DomsGarage.Models;

/// <summary>
/// A mechanic working at Dom's Garage.
/// Simple entity — no auth, no hierarchy, no shift scheduling.
/// Walkthrough note: intentionally thin. The monolith keeps things simple.
/// </summary>
public class Mechanic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    // Navigation properties for EF Core
    public ICollection<Job> Jobs { get; set; } = [];

    // LAB Step 1: Add navigation property for ServiceRecords here
    // public ICollection<ServiceRecord> ServiceRecords { get; set; } = [];
}
