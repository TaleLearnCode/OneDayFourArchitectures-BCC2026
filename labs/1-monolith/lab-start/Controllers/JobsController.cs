using DomsGarage.Models;
using DomsGarage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomsGarage.Controllers;

/// <summary>
/// REST endpoints for job management.
/// Includes the CloseJob endpoint which triggers the auto-status business rule.
/// </summary>
[ApiController]
[Route("api/jobs")]
public class JobsController(JobService jobService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Job>>> GetAll() =>
        Ok(await jobService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Job>> GetById(int id)
    {
        Job? job = await jobService.GetByIdAsync(id);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet("car/{carId:int}")]
    public async Task<ActionResult<List<Job>>> GetByCar(int carId) =>
        Ok(await jobService.GetByCarIdAsync(carId));

    [HttpPost]
    public async Task<ActionResult<Job>> Create(Job job)
    {
        Job created = await jobService.CreateAsync(job);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Closes a job and auto-updates the car status if all jobs for that car are done.
    /// This is the JobService business rule made visible as an endpoint.
    /// </summary>
    [HttpPatch("{id:int}/close")]
    public async Task<ActionResult<Job>> Close(int id)
    {
        Job? job = await jobService.CloseJobAsync(id);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await jobService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
