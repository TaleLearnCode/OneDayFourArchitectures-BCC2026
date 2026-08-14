using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;
using TorettoMotors.DAL.Entities;
using TorettoMotors.DAL.Repositories.Interfaces;

namespace TorettoMotors.BLL.Services.Implementations;

public class MaintenancePlanService : IMaintenancePlanService
{
	private readonly IMaintenancePlanRepository _maintenancePlanRepository;

	public MaintenancePlanService(IMaintenancePlanRepository maintenancePlanRepository)
	{
		_maintenancePlanRepository = maintenancePlanRepository;
	}

	public async Task<MaintenancePlanDto?> GetMaintenancePlanByIdAsync(int id)
	{
		var plan = await _maintenancePlanRepository.GetByIdAsync(id);
		return plan == null ? null : MapToDto(plan);
	}

	public async Task<IEnumerable<MaintenancePlanDto>> GetAllMaintenancePlansAsync()
	{
		var plans = await _maintenancePlanRepository.GetAllAsync();
		return plans.Select(MapToDto);
	}

	public async Task<IEnumerable<MaintenancePlanDto>> GetMaintenancePlansByCustomerIdAsync(int customerId)
	{
		var plans = await _maintenancePlanRepository.GetByCustomerIdAsync(customerId);
		return plans.Select(MapToDto);
	}

	public async Task<MaintenancePlanDto> CreateMaintenancePlanAsync(MaintenancePlanDto plan)
	{

		ValidateEntity(plan);

		var entity = new MaintenancePlanEntity
		{
			CustomerId = plan.CustomerId,
			Name = plan.Name,
			Description = plan.Description,
			MonthlyPrice = plan.MonthlyPrice,
			StartDate = plan.StartDate,
			Status = plan.Status ?? "Active"
		};

		var created = await _maintenancePlanRepository.AddAsync(entity);
		return MapToDto(created);
	}

	public async Task<MaintenancePlanDto> UpdateMaintenancePlanAsync(MaintenancePlanDto plan)
	{
		ValidateEntity(plan);

		var entity = new MaintenancePlanEntity
		{
			Id = plan.Id,
			CustomerId = plan.CustomerId,
			Name = plan.Name,
			Description = plan.Description,
			MonthlyPrice = plan.MonthlyPrice,
			StartDate = plan.StartDate,
			Status = plan.Status ?? "Active"
		};

		var updated = await _maintenancePlanRepository.UpdateAsync(entity);
		return MapToDto(updated);
	}

	private static void ValidateEntity(MaintenancePlanDto plan)
	{
		// Same validations as Create
		if (plan.MonthlyPrice <= 0)
			throw new ArgumentException("Monthly price must be greater than zero");

		if (string.IsNullOrWhiteSpace(plan.Name))
			throw new ArgumentException("Plan name is required");
		if (string.IsNullOrWhiteSpace(plan.Description))
			throw new ArgumentException("Plan description is required");

		if (plan.StartDate.Date < DateTime.Today)
			throw new ArgumentException("Start date must be today or in the future");
	}

	public async Task<bool> DeleteMaintenancePlanAsync(int id)
	{
		return await _maintenancePlanRepository.DeleteAsync(id);
	}

	private static MaintenancePlanDto MapToDto(MaintenancePlanEntity entity)
	{
		return new MaintenancePlanDto
		{
			Id = entity.Id,
			CustomerId = entity.CustomerId,
			Name = entity.Name,
			Description = entity.Description,
			MonthlyPrice = entity.MonthlyPrice,
			StartDate = entity.StartDate,
			Status = entity.Status
		};
	}

	public async Task<IEnumerable<MaintenancePlanDto>> GetActiveMaintenancePlansByCustomerIdAsync(int customerId)
	{
		var plans = await _maintenancePlanRepository.GetByCustomerIdAsync(customerId);

		// Filter to only "Active" status
		return plans
				.Where(p => p.Status == "Active" && p.StartDate.Date <= DateTime.Today)
				.Select(MapToDto);
	}

	public async Task<MaintenancePlanDto> RenewMaintenancePlanAsync(int planId)
	{
		var plan = await _maintenancePlanRepository.GetByIdAsync(planId);
		if (plan == null)
			throw new ArgumentException($"Plan with ID {planId} not found");

		// Set status to "Renewal Pending" to signal it's ready to renew
		plan.Status = "Renewal Pending";

		var updated = await _maintenancePlanRepository.UpdateAsync(plan);
		return MapToDto(updated);
	}

	public async Task<MaintenancePlanDto> RenewAsync(int planId, DateTime newStartDate)
	{
		// Validate new start date
		if (newStartDate.Date < DateTime.Today)
			throw new ArgumentException("New start date must be today or in the future");

		var plan = await _maintenancePlanRepository.GetByIdAsync(planId);
		if (plan == null)
			throw new ArgumentException($"Plan with ID {planId} not found");

		// Update start date and status to active
		plan.StartDate = newStartDate;
		plan.Status = "Active";

		var updated = await _maintenancePlanRepository.UpdateAsync(plan);
		return MapToDto(updated);
	}
}
