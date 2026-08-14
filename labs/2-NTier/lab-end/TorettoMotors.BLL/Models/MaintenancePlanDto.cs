namespace TorettoMotors.BLL.Models;

public class MaintenancePlanDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public DateTime StartDate { get; set; }
    public required string Status { get; set; }
}
