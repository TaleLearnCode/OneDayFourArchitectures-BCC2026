using CiphersGrid.CrewService.DTOs;
using CiphersGrid.CrewService.Models;
using CiphersGrid.CrewService.Repositories;
using CiphersGrid.SharedKernel.DTOs;

namespace CiphersGrid.CrewService.Services;

public class CrewService(DriverRepository driverRepository)
{
    public async Task<DriverDto?> GetDriverAsync(Guid driverId)
    {
        var driver = await driverRepository.GetByIdAsync(driverId);
        return driver is null ? null : MapToDto(driver);
    }

    public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
    {
        var drivers = await driverRepository.GetAllAsync();
        return drivers.Select(MapToDto);
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverRequest request)
    {
        var driver = new Driver
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            LicenseNumber = request.LicenseNumber,
            IsActive = true
        };

        var created = await driverRepository.AddAsync(driver);
        return MapToDto(created);
    }

    private static DriverDto MapToDto(Driver driver)
    {
        return new DriverDto(
            new(driver.Id),
            driver.FirstName,
            driver.LastName,
            driver.LicenseNumber,
            driver.IsActive
        );
    }
}
