using Microsoft.EntityFrameworkCore;
using TorettoMotors.DAL.Context;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.DAL.Repositories.Implementations;

public class MaintenancePlanRepository : IMaintenancePlanRepository
{
    private readonly TorettoDbContext _context;

    public MaintenancePlanRepository(TorettoDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenancePlanEntity?> GetByIdAsync(int id)
    {
        return await _context.MaintenancePlans
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MaintenancePlanEntity>> GetAllAsync()
    {
        return await _context.MaintenancePlans
            .Include(m => m.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenancePlanEntity>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.MaintenancePlans
            .Include(m => m.Customer)
            .Where(m => m.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<MaintenancePlanEntity> AddAsync(MaintenancePlanEntity plan)
    {
        _context.MaintenancePlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<MaintenancePlanEntity> UpdateAsync(MaintenancePlanEntity plan)
    {
        _context.MaintenancePlans.Update(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var plan = await _context.MaintenancePlans.FindAsync(id);
        if (plan == null)
            return false;

        _context.MaintenancePlans.Remove(plan);
        await _context.SaveChangesAsync();
        return true;
    }
}
