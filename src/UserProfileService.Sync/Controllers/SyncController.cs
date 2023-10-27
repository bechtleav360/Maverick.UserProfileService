using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Controllers;

/// <summary>
///     The controller is used to start a synchronization run.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SyncController : ControllerBase
{
    private readonly ILogger<SyncController> _logger;
    private readonly ISynchronizationService _synchronizationService;

    /// <summary>
    ///     Creates an instance of the object <see cref="SyncController" />.
    /// </summary>
    /// <param name="synchronizationService">The service to handle the synchronization process.</param>
    /// <param name="logger">The logger.</param>
    public SyncController(ISynchronizationService synchronizationService, ILogger<SyncController> logger)
    {
        _synchronizationService = synchronizationService;
        _logger = logger;
    }

    /// <summary>
    ///     The method is used to start a synchronization run.
    /// </summary>
    /// <response code="202">If the request was successful and the response body contains a status.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <param name="schedule">Indicates if the sync process is initiated by scheduler.</param>
    /// <param name="correlationId">The correlationId for the synchronization run.</param>
    /// <param name="cancellationToken">The cancellationToken to cancel the synchronization operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an <see cref="IActionResult" /> that contains
    ///     the saga id when the run was successful.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Start(
        [FromQuery] bool schedule = true,
        [FromHeader(Name = Constants.HeaderNameCorrelationId)] string correlationId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        try
        {
            Guid start = await _synchronizationService.StartSynchronizationAsync(
                correlationId,
                schedule,
                cancellationToken);

            var result = new AcceptedAtRouteResult(
                nameof(ProcessController.GetProcessAsync),
                new
                {
                    id = start,
                    controller = "Process"
                },
                null);

            return result;
        }
        catch (InvalidOperationException e)
        {
            _logger.LogErrorMessage(e, e.Message, LogHelpers.Arguments());

            // new Forbid(e.Message) can not be used, because authentication is not implemented.
            return new ObjectResult(e.Message)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
        finally
        {
            _logger.ExitMethod();
        }
    }
}
