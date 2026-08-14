using TorettoMotors.BLL.Models;

namespace TorettoMotors.BLL.Services.Interfaces;

public interface IVehicleService
{
    Task<VehicleDto?> GetVehicleByIdAsync(int id);
    Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
    Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerIdAsync(int customerId);
    Task<VehicleDto> CreateVehicleAsync(VehicleDto vehicle);
    Task<VehicleDto> UpdateVehicleAsync(VehicleDto vehicle);
    Task<bool> DeleteVehicleAsync(int id);
}
