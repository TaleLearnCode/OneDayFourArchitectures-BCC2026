using CiphersGrid.AlertService.DTOs;
using CiphersGrid.AlertService.Models;
using CiphersGrid.AlertService.Repositories;
using CiphersGrid.SharedKernel.Enums;

namespace CiphersGrid.AlertService.Services;

public class AlertService(AlertRepository alertRepository)
{
	public async Task<AlertResponseDto> CreateAlertAsync(
			Guid raceId,
			Guid driverId,
			string alertType,
			string severity,
			string message)
	{
		if (!Enum.TryParse<AlertType>(alertType, true, out var type))
			type = AlertType.Broadcast;

		if (!Enum.TryParse<AlertSeverity>(severity, true, out var sev))
			sev = AlertSeverity.Low;

		var alert = new Alert
		{
			RaceId = raceId,
			DriverId = driverId,
			AlertType = type,
			Severity = sev,
			Message = message
		};

		var created = await alertRepository.AddAsync(alert);
		return MapToDto(created);
	}

	public async Task<IEnumerable<AlertResponseDto>> GetAlertsForRaceAsync(Guid raceId)
	{
		var alerts = await alertRepository.GetByRaceIdAsync(raceId);
		return alerts.Select(MapToDto);
	}

	public async Task<IEnumerable<AlertResponseDto>> GetAllAlertsAsync()
	{
		var alerts = await alertRepository.GetAllAsync();
		return alerts.Select(MapToDto);
	}

	public async Task<AlertResponseDto?> AcknowledgeAlertAsync(Guid alertId)
	{
		var alert = await alertRepository.GetByIdAsync(alertId);
		if (alert is null) return null;

		alert.IsAcknowledged = true;
		await alertRepository.UpdateAsync(alert);
		return MapToDto(alert);
	}

	private static AlertResponseDto MapToDto(Alert alert)
	{
		return new AlertResponseDto(
				alert.Id,
				alert.RaceId,
				alert.DriverId,
				alert.AlertType.ToString(),
				alert.Severity.ToString(),
				alert.Message,
				alert.IssuedAt,
				alert.IsAcknowledged
		);
	}
}