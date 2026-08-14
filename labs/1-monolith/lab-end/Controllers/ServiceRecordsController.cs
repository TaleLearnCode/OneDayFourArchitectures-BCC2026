using DomsGarage.Models;
using DomsGarage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomsGarage.Controllers;

/// <summary>
/// REST endpoints for service record logging.
/// Exposes ServiceRecordService as HTTP endpoints via ASP.NET Core routing.
/// </summary>
[ApiController]
[Route("api/servicerecords")]
public class ServiceRecordsController(ServiceRecordService serviceRecordService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<ServiceRecord>>> GetAll() =>
			Ok(await serviceRecordService.GetAllAsync());

	[HttpGet("{id:int}")]
	public async Task<ActionResult<ServiceRecord>> GetById(int id)
	{
		ServiceRecord? record = await serviceRecordService.GetByIdAsync(id);
		return record is null ? NotFound() : Ok(record);
	}

	[HttpGet("car/{carId:int}")]
	public async Task<ActionResult<List<ServiceRecord>>> GetByCar(int carId) =>
			Ok(await serviceRecordService.GetByCarIdAsync(carId));

	[HttpPost]
	public async Task<ActionResult<ServiceRecord>> Create(ServiceRecord record)
	{
		ServiceRecord created = await serviceRecordService.CreateAsync(record);
		return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult<ServiceRecord>> Update(int id, ServiceRecord record)
	{
		ServiceRecord? updated = await serviceRecordService.UpdateAsync(id, record);
		return updated is null ? NotFound() : Ok(updated);
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		bool deleted = await serviceRecordService.DeleteAsync(id);
		return deleted ? NoContent() : NotFound();
	}
}