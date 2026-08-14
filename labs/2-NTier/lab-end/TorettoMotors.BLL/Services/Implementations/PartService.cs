using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.BLL.Services.Implementations;

public class PartService : IPartService
{
    private readonly IPartRepository _partRepository;

    public PartService(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<PartDto?> GetPartByIdAsync(int id)
    {
        var part = await _partRepository.GetByIdAsync(id);
        return part == null ? null : MapToDto(part);
    }

    public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
    {
        var parts = await _partRepository.GetAllAsync();
        return parts.Select(MapToDto);
    }

    public async Task<PartDto> CreatePartAsync(PartDto part)
    {
        var entity = new PartEntity
        {
            Name = part.Name,
            Category = part.Category,
            UnitCost = part.UnitCost,
            StockQuantity = part.StockQuantity
        };

        var created = await _partRepository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<PartDto> UpdatePartAsync(PartDto part)
    {
        var entity = new PartEntity
        {
            Id = part.Id,
            Name = part.Name,
            Category = part.Category,
            UnitCost = part.UnitCost,
            StockQuantity = part.StockQuantity
        };

        var updated = await _partRepository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeletePartAsync(int id)
    {
        return await _partRepository.DeleteAsync(id);
    }

    private static PartDto MapToDto(PartEntity entity)
    {
        return new PartDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            UnitCost = entity.UnitCost,
            StockQuantity = entity.StockQuantity
        };
    }
}
