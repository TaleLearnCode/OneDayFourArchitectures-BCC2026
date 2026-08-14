using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.BLL.Services.Implementations;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        return vehicle == null ? null : MapToDto(vehicle);
    }

    public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        return vehicles.Select(MapToDto);
    }

    public async Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerIdAsync(int customerId)
    {
        var vehicles = await _vehicleRepository.GetByCustomerIdAsync(customerId);
        return vehicles.Select(MapToDto);
    }

    public async Task<VehicleDto> CreateVehicleAsync(VehicleDto vehicle)
    {
        var entity = new VehicleEntity
        {
            CustomerId = vehicle.CustomerId,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate,
            Mileage = vehicle.Mileage
        };

        var created = await _vehicleRepository.AddAsync(entity);
        return MapToDto(created);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(VehicleDto vehicle)
    {
        var entity = new VehicleEntity
        {
            Id = vehicle.Id,
            CustomerId = vehicle.CustomerId,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            LicensePlate = vehicle.LicensePlate,
            Mileage = vehicle.Mileage
        };

        var updated = await _vehicleRepository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        return await _vehicleRepository.DeleteAsync(id);
    }

    private static VehicleDto MapToDto(VehicleEntity entity)
    {
        return new VehicleDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            LicensePlate = entity.LicensePlate,
            Mileage = entity.Mileage
        };
    }
}
