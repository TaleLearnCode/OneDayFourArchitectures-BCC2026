using Microsoft.EntityFrameworkCore;
using TorettoMotors.DAL.Context;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.DAL.Repositories.Implementations;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly TorettoDbContext _context;

    public InvoiceRepository(TorettoDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceEntity?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<InvoiceEntity>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<InvoiceEntity>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<InvoiceEntity> AddAsync(InvoiceEntity invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<InvoiceEntity> UpdateAsync(InvoiceEntity invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return false;

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return true;
    }
}
