using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.BLL.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        return invoice == null ? null : MapToDto(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        return invoices.Select(MapToDto);
    }

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesByCustomerIdAsync(int customerId)
    {
        var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId);
        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceDto invoice)
    {
        if (invoice.TotalAmount <= 0)
            throw new ArgumentException("Invoice total must be greater than zero");

        var entity = new InvoiceEntity
        {
            CustomerId = invoice.CustomerId,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status ?? "Pending"
        };

        var created = await _invoiceRepository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<InvoiceDto> UpdateInvoiceAsync(InvoiceDto invoice)
    {
        if (invoice.TotalAmount <= 0)
            throw new ArgumentException("Invoice total must be greater than zero");

        var entity = new InvoiceEntity
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status ?? "Pending"
        };

        var updated = await _invoiceRepository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteInvoiceAsync(int id)
    {
        return await _invoiceRepository.DeleteAsync(id);
    }

    private static InvoiceDto MapToDto(InvoiceEntity entity)
    {
        return new InvoiceDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            InvoiceDate = entity.InvoiceDate,
            TotalAmount = entity.TotalAmount,
            Status = entity.Status
        };
    }
}
