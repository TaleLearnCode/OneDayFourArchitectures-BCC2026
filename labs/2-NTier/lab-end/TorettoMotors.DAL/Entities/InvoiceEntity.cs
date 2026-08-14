namespace TorettoMotors.DAL.Entities;

public class InvoiceEntity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }

    public virtual CustomerEntity? Customer { get; set; }
}
