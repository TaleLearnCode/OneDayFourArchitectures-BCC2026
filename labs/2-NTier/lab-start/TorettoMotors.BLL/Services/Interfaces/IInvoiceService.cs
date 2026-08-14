using TorettoMotors.BLL.Models;

namespace TorettoMotors.BLL.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
    Task<IEnumerable<InvoiceDto>> GetInvoicesByCustomerIdAsync(int customerId);
    Task<InvoiceDto> CreateInvoiceAsync(InvoiceDto invoice);
    Task<InvoiceDto> UpdateInvoiceAsync(InvoiceDto invoice);
    Task<bool> DeleteInvoiceAsync(int id);
}
