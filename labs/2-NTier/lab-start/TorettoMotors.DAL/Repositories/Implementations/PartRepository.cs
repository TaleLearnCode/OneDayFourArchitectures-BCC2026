using Microsoft.EntityFrameworkCore;
using TorettoMotors.DAL.Context;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.DAL.Repositories.Implementations;

public class PartRepository : IPartRepository
{
    private readonly TorettoDbContext _context;

    public PartRepository(TorettoDbContext context)
    {
        _context = context;
    }

    public async Task<PartEntity?> GetByIdAsync(int id)
    {
        return await _context.Parts.FindAsync(id);
    }

    public async Task<IEnumerable<PartEntity>> GetAllAsync()
    {
        return await _context.Parts.ToListAsync();
    }

    public async Task<PartEntity> AddAsync(PartEntity part)
    {
        _context.Parts.Add(part);
        await _context.SaveChangesAsync();
        return part;
    }

    public async Task<PartEntity> UpdateAsync(PartEntity part)
    {
        _context.Parts.Update(part);
        await _context.SaveChangesAsync();
        return part;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part == null)
            return false;

        _context.Parts.Remove(part);
        await _context.SaveChangesAsync();
        return true;
    }
}
