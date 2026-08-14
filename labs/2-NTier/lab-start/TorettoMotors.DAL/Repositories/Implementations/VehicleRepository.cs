using Microsoft.EntityFrameworkCore;
using TorettoMotors.DAL.Context;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.DAL.Repositories.Implementations;

public class VehicleRepository : IVehicleRepository
{
    private readonly TorettoDbContext _context;

    public VehicleRepository(TorettoDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleEntity?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<VehicleEntity>> GetAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleEntity>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .Where(v => v.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<VehicleEntity> AddAsync(VehicleEntity vehicle)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return vehicle;
    }

    public async Task<VehicleEntity> UpdateAsync(VehicleEntity vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
        return vehicle;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
            return false;

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
        return true;
    }
}
