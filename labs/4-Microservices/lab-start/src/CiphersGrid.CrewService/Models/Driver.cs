using CiphersGrid.SharedKernel.Ids;

namespace CiphersGrid.CrewService.Models;

public class Driver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string LicenseNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CrewMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid DriverId { get; set; }
    public required string Role { get; set; }
    public required string ContactInfo { get; set; }
}
