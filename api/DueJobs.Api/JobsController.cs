using Microsoft.AspNetCore.Mvc;

namespace DueJobs.Api;

[ApiController]
[Route("jobs")]
public sealed class JobsController(IJobRepository jobs, JobService service) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(jobs.GetAll().OrderBy(j => j.ScheduledAt).ThenBy(j => j.Id));

    [HttpPost]
    public IActionResult Create(CreateJobRequest input)
    {
        try
        {
            var job = service.Create(input.Title, input.ScheduledAt, input.Kind, input.Target);
            return Created($"/jobs/{job.Id}", job);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("{id:guid}/cancel")]
    public IActionResult Cancel(Guid id)
    {
        var job = service.Cancel(id);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost("process-due")]
    public IActionResult ProcessDue() => Ok(new { processed = service.ProcessDue() });
}
