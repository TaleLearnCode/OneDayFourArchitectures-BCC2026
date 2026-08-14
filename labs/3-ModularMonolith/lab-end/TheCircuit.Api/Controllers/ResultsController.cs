using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.DTOs;

namespace TheCircuit.Api.Controllers;

/// <summary>
/// Race Results endpoints.
/// Hosted in the API layer; delegates to IResultsService (Results module).
/// 
/// *** TEACHING MOMENT: Cross-Module Data Composition ***
/// 
/// This controller demonstrates how modules collaborate through interfaces without
/// reaching into each other's internals:
/// 
/// - IResultsService returns RaceResultDto with participant name populated
/// - ResultsController injects BOTH IResultsService and IParticipantService
/// - When fetching results, we call IParticipantService.GetRacerAsync() for each result
/// - This shows: Results owns race data; Participants owns racer data; both are accessed
///   through contracts defined in TheCircuit.Shared
/// 
/// WHY THIS MATTERS:
/// In a monolith N-Tier design, a single SQL JOIN would fetch both tables in one query.
/// Here, we make multiple calls to GetRacerAsync(). This is the performance/isolation tradeoff.
/// It's the same tradeoff microservices make — but entirely in-process.
/// 
/// Facilitator note: Point to this controller during the code walkthrough. Emphasize:
/// "Results doesn't reference Participants. It only knows IParticipantService from Shared.
///  The compiler enforces this — try to add a direct reference and the build breaks."
/// </summary>
[ApiController]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly IResultsService _resultsService;
    private readonly IParticipantService _participantService;

    public ResultsController(IResultsService resultsService, IParticipantService participantService)
    {
        _resultsService = resultsService;
        _participantService = participantService;
    }

    /// <summary>
    /// GET /api/results/events/{eventId}
    /// Fetches race results for a specific event (leaderboard).
    /// Results are sorted by finish position.
    /// 
    /// Cross-module composition: Enriches results with racer names from Participants module.
    /// </summary>
    [HttpGet("events/{eventId:int}")]
    [ProducesResponseType(typeof(IEnumerable<RaceResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RaceResultDto>>> GetResultsForEvent(int eventId)
    {
        // Step 1: Results module fetches race results (owns lap times, positions)
        var results = await _resultsService.GetResultsForEventAsync(eventId);
        if (results == null || !results.Any())
            return Ok(new List<RaceResultDto>());

        // Step 2: For each result, fetch racer details from Participants module
        // TEACHING ANNOTATION: This is a module boundary crossing via interface.
        // IParticipantService is the contract; we never see RacerRepository or ParticipantsDbContext.
        var enrichedResults = new List<RaceResultDto>();
        foreach (var result in results)
        {
            var racer = await _participantService.GetRacerAsync(result.RacerId);
            enrichedResults.Add(result with { ParticipantName = racer?.FullName ?? "Unknown" });
        }

        return Ok(enrichedResults);
    }

    /// <summary>
    /// GET /api/results/racers/{racerId}
    /// Fetches all results for a specific racer (career history).
    /// Results are sorted by event date (most recent first).
    /// 
    /// Demonstrates same cross-module composition pattern as GetResultsForEvent().
    /// </summary>
    [HttpGet("racers/{racerId:int}")]
    [ProducesResponseType(typeof(IEnumerable<RaceResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RaceResultDto>>> GetResultsForRacer(int racerId)
    {
        var results = await _resultsService.GetResultsForRacerAsync(racerId);
        if (results == null || !results.Any())
            return Ok(new List<RaceResultDto>());

        // Enrich with racer name (will be same for all results, but pattern is consistent)
        var racer = await _participantService.GetRacerAsync(racerId);
        var racerName = racer?.FullName ?? "Unknown";

        var enrichedResults = results.Select(r => r with { ParticipantName = racerName }).ToList();
        return Ok(enrichedResults);
    }

    /// <summary>
    /// GET /api/results/{resultId}
    /// Fetches a single race result by ID.
    /// </summary>
    [HttpGet("{resultId:int}")]
    [ProducesResponseType(typeof(RaceResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceResultDto>> GetById(int resultId)
    {
        var result = await _resultsService.GetResultAsync(resultId);
        if (result == null)
            return NotFound();

        // Enrich with racer name
        var racer = await _participantService.GetRacerAsync(result.RacerId);
        var enrichedResult = result with { ParticipantName = racer?.FullName ?? "Unknown" };

        return Ok(enrichedResult);
    }
}
