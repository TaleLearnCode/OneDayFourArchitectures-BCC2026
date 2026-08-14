using TorettoMotors.BLL.Models;

namespace TorettoMotors.BLL.Services.Interfaces;

public interface IMaintenancePlanService
{
	Task<MaintenancePlanDto?> GetMaintenancePlanByIdAsync(int id);
	Task<IEnumerable<MaintenancePlanDto>> GetAllMaintenancePlansAsync();
	Task<IEnumerable<MaintenancePlanDto>> GetMaintenancePlansByCustomerIdAsync(int customerId);
	Task<MaintenancePlanDto> CreateMaintenancePlanAsync(MaintenancePlanDto plan);
	Task<MaintenancePlanDto> UpdateMaintenancePlanAsync(MaintenancePlanDto plan);
	Task<bool> DeleteMaintenancePlanAsync(int id);

	/// <summary>
	/// Get all active maintenance plans for a customer.
	/// </summary>
	Task<IEnumerable<MaintenancePlanDto>> GetActiveMaintenancePlansByCustomerIdAsync(int customerId);

	/// <summary>
	/// Mark a maintenance plan as pending renewal (called on annual anniversary).
	/// </summary>
	Task<MaintenancePlanDto> RenewMaintenancePlanAsync(int planId);

	/// <summary>
	/// Renew a maintenance plan by setting a new start date and activating it.
	/// </summary>
	Task<MaintenancePlanDto> RenewAsync(int planId, DateTime newStartDate);
}
