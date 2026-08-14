namespace TheCircuit.Participants.Models;

internal class Racer
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string LicenseNumber { get; set; }
    public required string TeamName { get; set; }
    public bool IsActive { get; set; }
}
