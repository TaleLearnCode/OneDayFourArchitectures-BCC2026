using Microsoft.EntityFrameworkCore;
using TorettoMotors.DAL.Context;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.DAL.Repositories.Implementations;

public class CustomerRepository(TorettoDbContext context) : ICustomerRepository
{
	private readonly TorettoDbContext _context = context;

	public async Task<CustomerEntity?> GetByIdAsync(int id)
	{
		return await _context.Customers
				.Include(c => c.Vehicles)
				.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<IEnumerable<CustomerEntity>> GetAllAsync()
	{
		return await _context.Customers
				.Include(c => c.Vehicles)
				.ToListAsync();
	}

	public async Task<CustomerEntity> AddAsync(CustomerEntity customer)
	{
		_context.Customers.Add(customer);
		await _context.SaveChangesAsync();
		return customer;
	}

	public async Task<CustomerEntity> UpdateAsync(CustomerEntity customer)
	{
		_context.Customers.Update(customer);
		await _context.SaveChangesAsync();
		return customer;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var customer = await _context.Customers.FindAsync(id);
		if (customer == null)
			return false;

		_context.Customers.Remove(customer);
		await _context.SaveChangesAsync();
		return true;
	}
}
