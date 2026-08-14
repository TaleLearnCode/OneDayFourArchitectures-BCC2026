using TorettoMotors.DAL.Entities;

namespace TorettoMotors.DAL.Repositories.Interfaces;

public interface IPartRepository
{
    Task<PartEntity?> GetByIdAsync(int id);
    Task<IEnumerable<PartEntity>> GetAllAsync();
    Task<PartEntity> AddAsync(PartEntity part);
    Task<PartEntity> UpdateAsync(PartEntity part);
    Task<bool> DeleteAsync(int id);
}
