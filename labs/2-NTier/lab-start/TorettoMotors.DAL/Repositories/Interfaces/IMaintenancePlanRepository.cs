using TorettoMotors.DAL.Entities;

namespace TorettoMotors.DAL.Repositories.Interfaces;

public interface IMaintenancePlanRepository
{
    Task<MaintenancePlanEntity?> GetByIdAsync(int id);
    Task<IEnumerable<MaintenancePlanEntity>> GetAllAsync();
    Task<IEnumerable<MaintenancePlanEntity>> GetByCustomerIdAsync(int customerId);
    Task<MaintenancePlanEntity> AddAsync(MaintenancePlanEntity plan);
    Task<MaintenancePlanEntity> UpdateAsync(MaintenancePlanEntity plan);
    Task<bool> DeleteAsync(int id);
}
