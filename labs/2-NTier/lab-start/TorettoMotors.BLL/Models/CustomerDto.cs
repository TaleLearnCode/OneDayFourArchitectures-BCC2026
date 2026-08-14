namespace TorettoMotors.BLL.Models;

public class CustomerDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public DateTime DateCreated { get; set; }
}
