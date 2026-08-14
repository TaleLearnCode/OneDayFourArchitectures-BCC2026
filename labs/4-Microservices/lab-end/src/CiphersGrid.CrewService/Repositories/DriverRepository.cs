using CiphersGrid.CrewService.Data;
using CiphersGrid.CrewService.Models;

namespace CiphersGrid.CrewService.Repositories;

public class DriverRepository(CrewDbContext context)
{
    public async Task<Driver?> GetByIdAsync(Guid id)
    {
        return await context.Drivers.FindAsync(id);
    }

    public async Task<IEnumerable<Driver>> GetAllAsync()
    {
        return context.Drivers.ToList();
    }

    public async Task<Driver> AddAsync(Driver driver)
    {
        await context.Drivers.AddAsync(driver);
        await context.SaveChangesAsync();
        return driver;
    }

    public async Task UpdateAsync(Driver driver)
    {
        context.Drivers.Update(driver);
        await context.SaveChangesAsync();
    }
}
