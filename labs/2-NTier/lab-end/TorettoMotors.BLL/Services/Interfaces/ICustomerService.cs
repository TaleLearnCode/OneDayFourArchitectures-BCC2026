using TorettoMotors.BLL.Models;

namespace TorettoMotors.BLL.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customer);
    Task<CustomerDto> UpdateCustomerAsync(CustomerDto customer);
    Task<bool> DeleteCustomerAsync(int id);
}
