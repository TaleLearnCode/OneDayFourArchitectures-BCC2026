namespace TorettoMotors.DAL.Entities;

public class CustomerEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public DateTime DateCreated { get; set; }

    public virtual ICollection<VehicleEntity> Vehicles { get; set; } = [];
    public virtual ICollection<InvoiceEntity> Invoices { get; set; } = [];
}
