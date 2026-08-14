namespace TorettoMotors.DAL.Entities;

public class MaintenancePlanEntity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = "Active";

    public virtual CustomerEntity? Customer { get; set; }
}
