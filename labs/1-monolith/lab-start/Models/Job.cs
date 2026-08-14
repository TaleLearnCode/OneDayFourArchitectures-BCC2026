namespace DomsGarage.Models;

/// <summary>
/// A repair job assigned to a car and a mechanic.
/// Status is derived: ClosedAt == null means the job is still open.
/// The CloseJob business rule in JobService is the primary walkthrough moment.
/// </summary>
public class Job
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int MechanicId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    // Derived — not stored. ClosedAt != null means complete.
    public bool IsComplete => ClosedAt.HasValue;

    // Navigation properties for EF Core — nullable so EF Core populates on load,
    // not required in POST/PUT request bodies
    public Car? Car { get; set; }
    public Mechanic? Mechanic { get; set; }
}
