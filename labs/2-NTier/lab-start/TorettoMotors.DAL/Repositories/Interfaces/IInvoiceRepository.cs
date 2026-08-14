using TorettoMotors.DAL.Entities;

namespace TorettoMotors.DAL.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<InvoiceEntity?> GetByIdAsync(int id);
    Task<IEnumerable<InvoiceEntity>> GetAllAsync();
    Task<IEnumerable<InvoiceEntity>> GetByCustomerIdAsync(int customerId);
    Task<InvoiceEntity> AddAsync(InvoiceEntity invoice);
    Task<InvoiceEntity> UpdateAsync(InvoiceEntity invoice);
    Task<bool> DeleteAsync(int id);
}
