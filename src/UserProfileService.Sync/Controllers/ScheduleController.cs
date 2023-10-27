using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Controllers;

/// <summary>
///     The controller is used to start a synchronization run.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IScheduleService _scheduleService;

    /// <summary>
    ///     Creates an instance of the object <see cref="ScheduleController" />.
    /// </summary>
    /// <param name="scheduleService">The service to handle the schedule of synchronization process.</param>
    /// <param name="logger">The logger.</param>
    public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    /// <summary>
    ///     Get the current schedule of synchronization process.
    /// </summary>
    /// <response code="200">If the request was successful and the response body contains the current schedule.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Current state of sync schedule.</returns>
    [HttpGet]
    public async Task<IActionResult> GetScheduleAsync(CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        SyncSchedule schedule = await _scheduleService.GetScheduleAsync(cancellationToken);

        _logger.LogInfoMessage("Successful retrieve schedule for synchronization process.", LogHelpers.Arguments());

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Successful retrieve schedule for synchronization process: {data}",
                JsonConvert.SerializeObject(schedule).AsArgumentList());
        }

        return _logger.ExitMethod(Ok(schedule));
    }

    /// <summary>
    ///     Change the schedule of synchronization process.
    /// </summary>
    /// <response code="200">If the request was successful and the response body contains the current schedule.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <param name="request">Request to change schedule with.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Current state of sync schedule.</returns>
    [HttpPut]
    public async Task<IActionResult> ChangeScheduleAsync(
        [FromBody] ScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Change current schedule of synchronization with request data: {data}",
                JsonConvert.SerializeObject(request).AsArgumentList());
        }

        string userid = HttpContext.GetUserId(_logger);

        _logger.LogInfoMessage("Got user id {userId} who try to change schedule.", userid.AsArgumentList());

        SyncSchedule schedule = await _scheduleService.ChangeScheduleAsync(request, userid, cancellationToken);

        _logger.LogInfoMessage("Successful change schedule for synchronization process.", LogHelpers.Arguments());

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Change current schedule of synchronization with current plan: {data}",
                JsonConvert.SerializeObject(schedule).AsArgumentList());
        }

        return _logger.ExitMethod(Ok(schedule));
    }
}
