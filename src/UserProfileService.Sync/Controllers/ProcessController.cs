using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.Views;

namespace UserProfileService.Sync.Controllers;

/// <summary>
///     The controller is used to start a synchronization run.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProcessController : ControllerBase
{
    private readonly ILogger<ProcessController> _logger;
    private readonly ISynchronizationService _synchronizationService;

    /// <summary>
    ///     Creates an instance of the object <see cref="ProcessController" />.
    /// </summary>
    /// <param name="synchronizationService">The service to handle the schedule of synchronization process.</param>
    /// <param name="logger">The logger.</param>
    public ProcessController(ISynchronizationService synchronizationService, ILogger<ProcessController> logger)
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
    [HttpGet("{id:guid}", Name = nameof(GetProcessAsync))]
    public async Task<IActionResult> GetProcessAsync(
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
    [HttpGet("{id:guid}/detail", Name = nameof(GetDetailedProcessAsync))]
    public async Task<IActionResult> GetDetailedProcessAsync(
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
    [HttpGet(Name = nameof(GetAllProcessesAsync))]
    [ProducesResponseType(typeof(PaginatedListResponse<ProcessView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProcessesAsync(
        [FromQuery] QueryObject queryObject = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        PaginatedList<ProcessView> processes =
            await _synchronizationService.GetAllProcessesAsync(queryObject, cancellationToken);

        var result = new PaginatedListResponse<ProcessView>(processes);

        return _logger.ExitMethod(Ok(result));
    }
}
