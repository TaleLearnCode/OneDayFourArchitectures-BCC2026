using CiphersGrid.AlertService.DTOs;
using Microsoft.AspNetCore.Mvc;
using AlertServiceClass = CiphersGrid.AlertService.Services.AlertService;

namespace CiphersGrid.AlertService.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(AlertServiceClass alertService) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> CreateAlert([FromBody] CreateAlertRequest request)
	{
		var alert = await alertService.CreateAlertAsync(
				request.RaceId,
				request.DriverId,
				request.AlertType,
				request.Severity,
				request.Message);

		return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, alert);
	}

	[HttpGet]
	public async Task<IActionResult> GetAlerts([FromQuery] Guid? raceId)
	{
		if (raceId.HasValue)
		{
			var alerts = await alertService.GetAlertsForRaceAsync(raceId.Value);
			return Ok(alerts);
		}

		var allAlerts = await alertService.GetAllAlertsAsync();
		return Ok(allAlerts);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetAlert(Guid id)
	{
		// Not implemented in this version, but could return single alert
		return Ok(new { id });
	}

	[HttpPut("{id}/acknowledge")]
	public async Task<IActionResult> AcknowledgeAlert(Guid id)
	{
		var alert = await alertService.AcknowledgeAlertAsync(id);
		if (alert is null) return NotFound();

		return Ok(alert);
	}

	[HttpGet("health")]
	public IActionResult Health()
	{
		return Ok("Alert Service is healthy");
	}
}