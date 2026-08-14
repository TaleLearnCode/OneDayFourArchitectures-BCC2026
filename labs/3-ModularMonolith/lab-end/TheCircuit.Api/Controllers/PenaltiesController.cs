using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;
using TheCircuit.SharedKernel.DTOs.Requests;

namespace TheCircuit.Api.Controllers;

/// <summary>
/// Race Penalties endpoints (SCAFFOLD FOR LAB TASK).
/// Hosted in the API layer; delegates to IPenaltyService (Penalties module).
/// 
/// *** PARTICIPANT LAB TASK ***
/// 
/// Participants will implement:
/// 1. POST /api/penalties endpoint to create a new penalty
/// 2. GET /api/penalties endpoint to list all penalties (stub shown below)
/// 3. IPenaltyService implementation in the Penalties module
/// 
/// The key architectural moment:
/// When POST /api/penalties is called, PenaltyService will:
/// - Persist the penalty to its own database (PenaltiesDbContext)
/// - Call IResultsService.ApplyPenaltyAsync() to update race results
/// - This demonstrates cross-module communication through interfaces
/// 
/// Before adding endpoints, create:
/// - TheCircuit.Penalties project
/// - Penalty entity and PenaltiesDbContext
/// - PenaltyService implementation (must inject IResultsService)
/// - PenaltiesModule registration in Program.cs
/// </summary>
[ApiController]
[Route("api/penalties")]
public class PenaltiesController : ControllerBase
{
    private readonly IPenaltyService _penaltyService;

    public PenaltiesController(IPenaltyService penaltyService)
    {
        _penaltyService = penaltyService;
    }

    /// <summary>
    /// GET /api/penalties
    /// Lists all penalties (admin view).
    /// Stub: participants may implement this as an enhancement.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PenaltyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetAll()
    {
        // PARTICIPANT TASK: Implement by calling _penaltyService
        // Placeholder for now
        var penalties = await Task.FromResult(new List<PenaltyDto>());
        return Ok(penalties);
    }

    /// <summary>
    /// POST /api/penalties
    /// Creates a new penalty for a racer in an event.
    /// 
    /// *** PRIMARY LAB ENDPOINT ***
    /// 
    /// Request body:
    /// {
    ///   "eventId": 1,
    ///   "racerId": 42,
    ///   "penaltyReason": "CourseCut",
    ///   "penaltyCostMs": 2500
    /// }
    /// 
    /// Expected behavior:
    /// 1. Validates inputs (eventId > 0, racerId > 0)
    /// 2. Calls _penaltyService.IssuePenaltyAsync()
    /// 3. PenaltyService persists penalty and calls IResultsService.ApplyPenaltyAsync()
    /// 4. Returns 201 Created with the new penalty details
    /// 
    /// Response:
    /// {
    ///   "penaltyId": 701,
    ///   "eventId": 1,
    ///   "racerId": 42,
    ///   "penaltyReason": "CourseCut",
    ///   "penaltyCostMs": 2500,
    ///   "appliedDate": "2026-06-15T11:30:00Z",
    ///   "status": "Issued"
    /// }
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PenaltyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PenaltyDto>> CreatePenalty([FromBody] CreatePenaltyRequest request)
    {
        // PARTICIPANT TASK: Implement validation and service call
        // - Validate request.EventId > 0 && request.RacerId > 0
        // - Validate penaltyReason is a valid PenaltyType
        // - Call _penaltyService.IssuePenaltyAsync()
        // - Return CreatedAtAction() with the new penalty

        // Placeholder implementation
        if (request.EventId <= 0 || request.RacerId <= 0)
            return BadRequest("EventId and RacerId must be positive");

        // TODO: Implement actual penalty creation
        var penalty = new PenaltyDto(0, request.EventId, request.RacerId, request.PenaltyReason, request.PenaltyCostMs, DateTime.UtcNow, "Issued");
        return CreatedAtAction(nameof(GetPenalty), new { penaltyId = penalty.PenaltyId }, penalty);
    }

    /// <summary>
    /// GET /api/penalties/{penaltyId}
    /// Fetches a single penalty by ID.
    /// Participants may implement this as an enhancement.
    /// </summary>
    [HttpGet("{penaltyId:int}")]
    [ProducesResponseType(typeof(PenaltyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PenaltyDto>> GetPenalty(int penaltyId)
    {
        var penalty = await _penaltyService.GetPenaltyAsync(penaltyId);
        if (penalty == null)
            return NotFound();
        return Ok(penalty);
    }

    /// <summary>
    /// GET /api/penalties/events/{eventId}
    /// Lists all penalties issued in a specific event.
    /// Participants may implement this as an enhancement.
    /// </summary>
    [HttpGet("events/{eventId:int}")]
    [ProducesResponseType(typeof(IEnumerable<PenaltyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetPenaltiesForEvent(int eventId)
    {
        var penalties = await _penaltyService.GetPenaltiesForEventAsync(eventId);
        return Ok(penalties);
    }

    /// <summary>
    /// GET /api/penalties/racers/{racerId}
    /// Lists all penalties issued to a specific racer across all events.
    /// Participants may implement this as an enhancement.
    /// </summary>
    [HttpGet("racers/{racerId:int}")]
    [ProducesResponseType(typeof(IEnumerable<PenaltyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetPenaltiesForRacer(int racerId)
    {
        var penalties = await _penaltyService.GetPenaltiesForRacerAsync(racerId);
        return Ok(penalties);
    }
}
