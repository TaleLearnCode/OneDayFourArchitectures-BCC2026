using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.BLL.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer == null ? null : MapToDto(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customer)
    {
        var entity = new CustomerEntity
        {
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            DateCreated = DateTime.UtcNow
        };

        var created = await _customerRepository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(CustomerDto customer)
    {
        var entity = new CustomerEntity
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            DateCreated = customer.DateCreated
        };

        var updated = await _customerRepository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        return await _customerRepository.DeleteAsync(id);
    }

    private static CustomerDto MapToDto(CustomerEntity entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone,
            DateCreated = entity.DateCreated
        };
    }
}
