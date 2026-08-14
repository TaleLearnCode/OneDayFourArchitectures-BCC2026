namespace TorettoMotors.BLL.Models;

public class InvoiceDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
}
