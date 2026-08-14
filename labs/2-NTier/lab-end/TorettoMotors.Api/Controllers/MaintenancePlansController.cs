using Microsoft.AspNetCore.Mvc;
using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;

namespace TorettoMotors.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenancePlansController : ControllerBase
{
	private readonly IMaintenancePlanService _maintenancePlanService;

	public MaintenancePlansController(IMaintenancePlanService maintenancePlanService)
	{
		_maintenancePlanService = maintenancePlanService;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<MaintenancePlanDto>>> GetAll()
	{
		var plans = await _maintenancePlanService.GetAllMaintenancePlansAsync();
		return Ok(plans);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<MaintenancePlanDto>> GetById(int id)
	{
		var plan = await _maintenancePlanService.GetMaintenancePlanByIdAsync(id);
		if (plan == null)
			return NotFound();
		return Ok(plan);
	}

	[HttpGet("customer/{customerId}")]
	public async Task<ActionResult<IEnumerable<MaintenancePlanDto>>> GetByCustomerId(int customerId)
	{
		var plans = await _maintenancePlanService.GetMaintenancePlansByCustomerIdAsync(customerId);
		return Ok(plans);
	}

	[HttpPost]
	public async Task<ActionResult<MaintenancePlanDto>> Create(MaintenancePlanDto plan)
	{
		try
		{
			var created = await _maintenancePlanService.CreateMaintenancePlanAsync(plan);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(int id, MaintenancePlanDto plan)
	{
		if (id != plan.Id)
			return BadRequest();

		try
		{
			var updated = await _maintenancePlanService.UpdateMaintenancePlanAsync(plan);
			return Ok(updated);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		var result = await _maintenancePlanService.DeleteMaintenancePlanAsync(id);
		if (!result)
			return NotFound();
		return NoContent();
	}

	[HttpPost("{id}/renew")]
	public async Task<ActionResult<MaintenancePlanDto>> Renew(int id)
	{
		try
		{
			var renewed = await _maintenancePlanService.RenewMaintenancePlanAsync(id);
			return Ok(renewed);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	[HttpGet("customer/{customerId}/active")]
	public async Task<ActionResult<IEnumerable<MaintenancePlanDto>>> GetActiveByCustomerId(int customerId)
	{
		var plans = await _maintenancePlanService.GetActiveMaintenancePlansByCustomerIdAsync(customerId);
		return Ok(plans);
	}
}