using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Proxy.Sync.Abstractions;
using UserProfileService.Proxy.Sync.Models;
using UserProfileService.Proxy.Sync.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     The controller is used to manage UPS-Sync processes.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SyncProcessController : ControllerBase
{
    private readonly ILogger<SyncProcessController> _logger;
    private readonly ISynchronizationService _synchronizationService;

    /// <summary>
    ///     Creates an instance of the object <see cref="SyncProcessController" />.
    /// </summary>
    /// <param name="synchronizationService">The service to handle the schedule of synchronization process.</param>
    /// <param name="logger">The logger.</param>
    public SyncProcessController(ISynchronizationService synchronizationService, ILogger<SyncProcessController> logger)
    {
        _synchronizationService = synchronizationService;
        _logger = logger;
    }

    /// <summary>
    ///     Return the sync process for the given id.
    /// </summary>
    /// <response code="200">If the request was successful and the response body contains a paginated list of sync processes.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <param name="id">Id of process to retrieve.</param>
    /// <param name="cancellationToken">The cancellationToken to cancel the synchronization operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an <see cref="IActionResult" /> that contains
    ///     the process.
    /// </returns>
    [HttpGet("{id:guid}", Name = nameof(GetSyncProcessAsync))]
    public async Task<IActionResult> GetSyncProcessAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ProcessView processView =
            await _synchronizationService.GetProcessAsync(id, cancellationToken);

        return _logger.ExitMethod(Ok(processView));
    }

    /// <summary>
    ///     Return a detailed sync process for the given id.
    /// </summary>
    /// <response code="200">If the request was successful and the response body contains a paginated list of sync processes.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <param name="id">Id of process to retrieve.</param>
    /// <param name="cancellationToken">The cancellationToken to cancel the synchronization operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an <see cref="IActionResult" /> that contains
    ///     the process.
    /// </returns>
    [HttpGet("{id:guid}/detail", Name = nameof(GetDetailedSyncProcessAsync))]
    public async Task<IActionResult> GetDetailedSyncProcessAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        ProcessDetail process =
            await _synchronizationService.GetDetailedProcessAsync(id, cancellationToken);

        return _logger.ExitMethod(Ok(process));
    }

    /// <summary>
    ///     Return a list containing all sync processes.
    /// </summary>
    /// <response code="200">If the request was successful and the response body contains a paginated list of sync processes.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">The cancellationToken to cancel the synchronization operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps an <see cref="IActionResult" /> that contains
    ///     the status.
    /// </returns>
    [HttpGet(Name = nameof(GetAllSyncProcessesAsync))]
    [ProducesResponseType(typeof(PaginatedListResponse<ProcessView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSyncProcessesAsync(
        [FromQuery] QueryObject queryObject = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        PaginatedListResponse<ProcessView> processes =
            await _synchronizationService.GetAllProcessesAsync(queryObject, cancellationToken);

        return _logger.ExitMethod(Ok(processes));
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
                nameof(GetSyncProcessAsync),
                new
                {
                    id = start,
                    controller = "SyncProcess"
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
