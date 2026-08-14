using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.Enums;
using TheCircuit.SharedKernel.Ids;
using CircuitEventId = TheCircuit.SharedKernel.Ids.EventId;

namespace TheCircuit.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/[controller]")]
public class PenaltiesController : ControllerBase
{
	private readonly IPenaltyService _penaltyService;

	public PenaltiesController(IPenaltyService penaltyService)
	{
		_penaltyService = penaltyService;
	}

	[HttpGet]
	public async Task<IActionResult> GetEventPenalties(int eventId)
	{
		var penalties = await _penaltyService.GetPenaltiesByEventAsync(new CircuitEventId(eventId));
		return Ok(penalties);
	}

	[HttpPost]
	public async Task<IActionResult> IssuePenalty(int eventId, [FromBody] IssuePenaltyRequest request)
	{
		var penaltyDto = new PenaltyDto(
				new PenaltyId(0),
				new CircuitEventId(eventId),
				new RacerId(request.RacerId),
				request.Reason,
				request.PenaltySeconds,
				DateTime.UtcNow,
				request.OfficialNotes,
				PenaltyStatus.Issued
		);

		var result = await _penaltyService.IssuePenaltyAsync(penaltyDto);
		return CreatedAtAction(nameof(GetPenalty), new { eventId, penaltyId = result.Id.Value }, result);
	}

	[HttpGet("{penaltyId}")]
	public async Task<IActionResult> GetPenalty(int eventId, int penaltyId)
	{
		var penalty = await _penaltyService.GetPenaltyByIdAsync(new PenaltyId(penaltyId));
		return penalty is null ? NotFound() : Ok(penalty);
	}
}

public class IssuePenaltyRequest
{
	public int RacerId { get; set; }
	public PenaltyReason Reason { get; set; }
	public int PenaltySeconds { get; set; }
	public required string OfficialNotes { get; set; }
}