namespace DomsGarage.Models;

/// <summary>
/// A permanent log of a completed service event at Dom's Garage.
/// Links a Car to the Mechanic who serviced it, with a description and date.
/// </summary>
public class ServiceRecord
{
	public int Id { get; set; }
	public int CarId { get; set; }
	public int MechanicId { get; set; }
	public string ServiceDescription { get; set; } = string.Empty;
	public DateTime DateCompleted { get; set; } = DateTime.UtcNow;
	public string? Notes { get; set; }

	// Navigation properties — EF Core populates these when you use .Include()
	public Car? Car { get; set; }
	public Mechanic? Mechanic { get; set; }
}