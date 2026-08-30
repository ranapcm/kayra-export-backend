using KayraExport.Log.Application.Interfaces;
using KayraExport.Log.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace KayraExport.Log.API.Controllers;

[ApiController]
[Route("api/v1/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly IEventLogRepository _repository;

    public LogsController(IEventLogRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<EventLogEntry>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EventLogEntry>>> GetRecent(
        [FromQuery] int count = 100,
        CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 500)
        {
            return BadRequest(
                "Count must be between 1 and 500.");
        }

        var logs = await _repository.GetRecentAsync(
            count,
            cancellationToken);

        return Ok(logs);
    }
}