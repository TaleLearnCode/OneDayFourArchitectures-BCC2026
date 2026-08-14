using TorettoMotors.DAL.Entities;

namespace TorettoMotors.DAL.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(int id);
    Task<IEnumerable<CustomerEntity>> GetAllAsync();
    Task<CustomerEntity> AddAsync(CustomerEntity customer);
    Task<CustomerEntity> UpdateAsync(CustomerEntity customer);
    Task<bool> DeleteAsync(int id);
}
