using Microsoft.AspNetCore.Mvc;

namespace TheCircuit.Api.Controllers;

/// <summary>
/// Health check endpoint.
/// Simple liveness probe for deployment pipelines and monitoring.
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// GET /health
    /// Returns a simple status indicating the API is running.
    /// No authentication or module interaction required.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetHealth()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
