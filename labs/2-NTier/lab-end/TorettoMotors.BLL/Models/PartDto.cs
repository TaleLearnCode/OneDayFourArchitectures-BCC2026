namespace TorettoMotors.BLL.Models;

public class PartDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public decimal UnitCost { get; set; }
    public int StockQuantity { get; set; }
}
