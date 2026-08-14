using TorettoMotors.DAL.Entities;

namespace TorettoMotors.DAL.Repositories.Interfaces;

public interface IVehicleRepository
{
    Task<VehicleEntity?> GetByIdAsync(int id);
    Task<IEnumerable<VehicleEntity>> GetAllAsync();
    Task<IEnumerable<VehicleEntity>> GetByCustomerIdAsync(int customerId);
    Task<VehicleEntity> AddAsync(VehicleEntity vehicle);
    Task<VehicleEntity> UpdateAsync(VehicleEntity vehicle);
    Task<bool> DeleteAsync(int id);
}
