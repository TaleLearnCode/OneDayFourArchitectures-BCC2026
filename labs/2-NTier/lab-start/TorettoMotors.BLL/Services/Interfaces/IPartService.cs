using TorettoMotors.BLL.Models;

namespace TorettoMotors.BLL.Services.Interfaces;

public interface IPartService
{
    Task<PartDto?> GetPartByIdAsync(int id);
    Task<IEnumerable<PartDto>> GetAllPartsAsync();
    Task<PartDto> CreatePartAsync(PartDto part);
    Task<PartDto> UpdatePartAsync(PartDto part);
    Task<bool> DeletePartAsync(int id);
}
