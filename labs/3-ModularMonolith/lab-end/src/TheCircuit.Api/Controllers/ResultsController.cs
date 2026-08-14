using Microsoft.AspNetCore.Mvc;
using TheCircuit.SharedKernel.Contracts;
using TheCircuit.SharedKernel.Ids;

namespace TheCircuit.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly IResultsService _resultsService;

    public ResultsController(IResultsService resultsService)
    {
        _resultsService = resultsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        var results = await _resultsService.GetResultsByEventAsync(new SharedKernel.Ids.EventId(eventId));
        return Ok(results.OrderBy(r => r.FinishPosition));
    }

    [HttpGet("{resultId}")]
    public async Task<IActionResult> GetResult(int resultId)
    {
        var result = await _resultsService.GetResultByIdAsync(new ResultId(resultId));
        return result is null ? NotFound() : Ok(result);
    }
}
