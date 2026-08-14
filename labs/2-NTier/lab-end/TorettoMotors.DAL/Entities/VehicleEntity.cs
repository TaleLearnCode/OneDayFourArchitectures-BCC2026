namespace TorettoMotors.DAL.Entities;

public class VehicleEntity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string LicensePlate { get; set; }
    public int Mileage { get; set; }

    public virtual CustomerEntity? Customer { get; set; }
}
