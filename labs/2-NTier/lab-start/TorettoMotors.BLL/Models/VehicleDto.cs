namespace TorettoMotors.BLL.Models;

public class VehicleDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string LicensePlate { get; set; }
    public int Mileage { get; set; }
}
