using Asp.Versioning;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Api.Common.Extensions;
using UserProfileService.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     Contains endpoints that should help tp debug or monitor UPS applications and their storage.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IAdminReadService _readService;

    public AdminController(
        ILogger<AdminController> logger,
        IAdminReadService readService)
    {
        _logger = logger;
        _readService = readService;
    }

    /// <summary>
    ///     Returns the projection state in the desired scope as a list of <c>ProjectionState</c> entries.
    /// </summary>
    /// <param name="streamNameFilter">Include all names of streams whose state should be retrieved.</param>
    /// <param name="paginationSettings">Includes settings for pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the projection state has been returned.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of roles</returns>
    [HttpGet("projectionState/service", Name = nameof(GetServiceProjectionState))]
    [ProducesResponseType(
        typeof(DictionaryResponseResult<string, IList<ProjectionState>>),
        StatusCodes.Status200OK)]
    [SwaggerDefaultValue(nameof(PaginationQueryObject.OrderedBy), nameof(ProjectionState.EventNumberVersion))]
    [SwaggerDefaultValue(nameof(PaginationQueryObject.Limit), 10)]
    [SwaggerDefaultValue(nameof(PaginationQueryObject.Offset), 0)]
    public async Task<IActionResult> GetServiceProjectionState(
        [FromQuery] string[] streamNameFilter = null,
        [FromQuery] PaginationQueryObject paginationSettings = null,
        CancellationToken cancellationToken = default)

    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Arguments: paginationSettings: {paginationSettings}",
                LogHelpers.Arguments(paginationSettings.ToLogString()));
        }

        GroupedProjectionState state = await _readService.GetServiceProjectionStateAsync(
            streamNameFilter,
            paginationSettings,
            cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(
                new DictionaryResponseResult<string, IList<ProjectionState>>
                {
                    Result = state,
                    Response = HttpContext.CreateListResponse(state.TotalCount, paginationSettings)
                });

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns the projection state in the desired scope as a list of <c>ProjectionState</c> entries.
    /// </summary>
    /// <param name="paginationSettings">Includes settings for pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the projection state has been returned.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of roles</returns>
    [HttpGet("projectionState/firstLevel", Name = nameof(GetFirstLevelProjectionState))]
    [ProducesResponseType(typeof(ListResponseResult<ProjectionState>), StatusCodes.Status200OK)]
    [SwaggerDefaultValue(nameof(PaginationQueryObject.OrderedBy), nameof(ProjectionState.EventNumberVersion))]
    [SwaggerDefaultValue(nameof(PaginationQueryObject.Limit), 10)]
    [SwaggerDefaultValue(nameof(PaginationQueryObject.Offset), 0)]
    public async Task<IActionResult> GetFirstLevelProjectionState(
        [FromQuery] PaginationQueryObject paginationSettings = null,
        CancellationToken cancellationToken = default)

    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Arguments: paginationSettings: {paginationSettings}",
                LogHelpers.Arguments(paginationSettings.ToLogString()));
        }

        IPaginatedList<ProjectionState> state = await _readService.GetFirstLevelProjectionStateAsync(
            paginationSettings,
            cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(state.ToListResponseResult(HttpContext, paginationSettings));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets a list of entries as projection state statistic.
    /// </summary>
    /// <remarks>
    ///     It returns a list of entries (each per projection, like first-level) representing the projection state statistic.
    ///     It contains information about average projection time, total amount of events, etc.
    /// </remarks>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the projection state has been returned.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of roles</returns>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps an <see cref="IActionResult" /> containing the
    ///     response of the current operation.
    /// </returns>
    [HttpGet("projectionState/all", Name = nameof(GetProjectionStateStatistics))]
    [ProducesResponseType(typeof(ListResponseResult<ProjectionState>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectionStateStatistics(
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IList<ProjectionStateStatisticEntry> entries = await _readService.GetProjectionStateStatisticAsync(
            cancellationToken);

        IActionResult value = ActionResultHelper.ToActionResult(entries);

        return _logger.ExitMethod(value);
    }
}
