using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Asp.Versioning;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Extensions;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     All methods related to queries.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0", Deprecated = false)]
public class QueriesController : ControllerBase
{
    private readonly ILogger<QueriesController> _logger;
    private readonly IReadService _readService;

    public QueriesController(ILoggerFactory loggerFactory, IReadService readService)
    {
        _readService = readService;
        _logger = loggerFactory.CreateLogger<QueriesController>();
    }

    /// <summary>
    ///     Gets the possible filter items of specified data view.
    /// </summary>
    /// <param name="viewFilter">
    ///     Determines returned filter object.<br />
    ///     (example: contains(users.displayName.keyValue.filter("ad").paginated(0,100),users.updatedAt.date))
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the view was found and the request processed successfully.</response>
    /// <response code="400">
    ///     If the requested view filter is malformed or if properties could not be found for estimated entity
    ///     type.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the view was not found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of groups the current user is assigned to.</returns>
    [HttpGet(Name = nameof(GetFiltersOfViewAsync))]
    [ProducesResponseType(typeof(ViewFilterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFiltersOfViewAsync(
        [FromQuery] [Required] string viewFilter,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ViewFilter: {viewFilter}",
                LogHelpers.Arguments(viewFilter));
        }

        var filterModels = new List<ViewFilterModel>();
        List<object> queryResult;

        try
        {
            filterModels.Parse(viewFilter);

            queryResult = await _readService.GetViewFilterLists(filterModels);
        }
        catch (SerializationException serializationException)
        {
            _logger.LogDebugMessage(
                serializationException,
                "Error occurred during parsing viewFilter = {viewFilter}. {serializationExceptionMessage}",
                LogHelpers.Arguments(viewFilter, serializationException.Message));

            // the value data key contains the value that could not be parsed.
            if (serializationException.Data["value"] != null)
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid view filter",
                        Detail =
                            $"Filter string invalid: \"{serializationException.Data["value"]}\" could not be parsed properly."
                    });
            }

            return _logger.ExitMethod(
                BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid view filter",
                        Detail = serializationException.Message
                    }));
        }
        catch (NotValidException notValid)
        {
            return _logger.ExitMethod(
                BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid view filter",
                        Detail = notValid.Message
                    }));
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error occurred during parsing viewFilter = {filterContent}. {errorMessage}",
                LogHelpers.Arguments(viewFilter, e.Message));

            return _logger.ExitMethod(
                Problem(
                    $"Internal error occurred while {nameof(GetFiltersOfViewAsync)}(). {e.Message}",
                    statusCode: StatusCodes.Status500InternalServerError));
        }

        var paginatedListResults = new List<PaginatedResponseResult>();

        for (var i = 0; i < queryResult.Count; i++)
        {
            var paginatedList = new PaginatedResponseResult
            {
                Result = queryResult[i] as IEnumerable,
                Response = new PaginatedResponse
                {
                    Count = (queryResult[i] as IList)?.Count ?? 0,
                    TotalAmount =
                        queryResult[i]
                            ?.GetType()
                            .GetProperty(nameof(PaginatedResponse.TotalAmount))
                            ?.GetValue(queryResult[i]) as long?
                        ?? (queryResult[i] as IList)?.Count ?? 0
                },
                ViewFilter = filterModels[i]
            };

            paginatedListResults.Add(paginatedList);
        }

        return _logger.ExitMethod(
            Ok(
                new ViewFilterResponse
                {
                    RawFilter = viewFilter,
                    RequestedFilters = paginatedListResults
                }));
    }
}
