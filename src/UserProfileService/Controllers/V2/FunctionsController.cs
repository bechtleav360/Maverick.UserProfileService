using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Api.Common.Abstractions;
using UserProfileService.Api.Common.Attributes;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Api.Common.Extensions;
using UserProfileService.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Configuration;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     All methods related to function.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class FunctionsController : ControllerBase
{
    private readonly ILogger<FunctionsController> _logger;
    private readonly IOperationHandler _operationHandler;
    private readonly IReadService _readService;

    public FunctionsController(
        ILoggerFactory loggerFactory,
        IReadService readService,
        IOperationHandler operationHandler)
    {
        _readService = readService;
        _operationHandler = operationHandler;
        _logger = loggerFactory.CreateLogger<FunctionsController>();
    }

    /// <summary>
    ///     Returns a list of functions.
    /// </summary>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the list with functions was found and the request processed successfully.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of functions.</returns>
    [HttpGet(Name = nameof(GetFunctionsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<FunctionBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFunctionsAsync(
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "QueryObject: {queryObject}",
                LogHelpers.Arguments(queryObject.ToLogString()));
        }

        IPaginatedList<FunctionBasic> functions =
            await _readService.GetFunctionsAsync<FunctionBasic>(queryObject, cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(functions.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns a list of functions.
    /// </summary>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the list with functions was found and the request processed successfully.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of functions.</returns>
    [HttpGet("view", Name = nameof(GetFunctionsViewAsync))]
    [ProducesResponseType(typeof(ListResponseResult<FunctionView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFunctionsViewAsync(
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "QueryObject: {queryObject}",
                LogHelpers.Arguments(queryObject.ToLogString()));
        }

        IPaginatedList<FunctionView> functions =
            await _readService.GetFunctionsAsync<FunctionView>(queryObject, cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(functions.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns a specified function.
    /// </summary>
    /// <param name="id">The id of the function to be returned.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the specified function was found.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the function could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A task representing the asynchronous read operation.</returns>
    [HttpGet("{id}", Name = nameof(GetFunctionAsync))]
    [ProducesResponseType(typeof(FunctionView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFunctionAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "FunctionsId: {functionsId}",
                LogHelpers.Arguments(id));
        }

        var function = await _readService.GetFunctionAsync<FunctionView>(id, cancellationToken);
        IActionResult result = ActionResultHelper.ToActionResult(function.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Creates a function.
    /// </summary>
    /// <param name="function">The function properties that can be set.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of functions.</returns>
    [HttpPost(Name = nameof(CreateFunctionAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateFunctionAsync(
        [FromBody] CreateFunctionRequest function,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "CreateFunctionRequest: {createFunctionRequest}",
                LogHelpers.Arguments(function.ToLogString()));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.CreateFunctionAsync(function, cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Deletes a single function.
    /// </summary>
    /// <param name="id">The id of the function which should be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The deleted function.</returns>
    [HttpDelete("{id}", Name = nameof(DeleteFunctionAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteFunctionAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "FunctionId: {functionId}",
                LogHelpers.Arguments(id));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.DeleteFunctionAsync(id, cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }
    
    /// <summary>
    ///     Assigns the specified profile to a least one object by the specified function.
    /// </summary>
    /// <param name="id">The function id which should be assigned to a profile.</param>
    /// <param name="profileId">The profileId which should be assigned to a function.</param>
    /// <param name="conditions">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">If the request was not processed successfully, because the body of the request was mal-formed.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">
    ///     If the request was not processed successfully, because either function or profile could not be
    ///     found.The response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The deleted function.</returns>
    [HttpPost("{id}/profiles/{profileId}", Name = nameof(AssignProfileToFunctionAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> AssignProfileToFunctionAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        [FromBody] RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "FunctionId: {functionId}, profileId: {profileId}, RangeCondition: {rangeCondition}",
                LogHelpers.Arguments(id, profileId, conditions.ToLogString()));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.AddProfileToFunctionAsync(
                profileId,
                ProfileKind.Unknown,
                id,
                conditions ?? Array.Empty<RangeCondition>(),
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Updates the profile assignments of the specified function.
    /// </summary>
    /// <param name="id">The function id of which the profile assignments should be updated.</param>
    /// <param name="assignments">The assignments to be updated.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">If the request was not processed successfully, because the body of the request was mal-formed.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">
    ///     If the request was not processed successfully, because either function or profiles could not be
    ///     found.The response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignments of the function.</returns>
    [HttpPut("{id}/profiles", Name = nameof(UpdateProfileToFunctionAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateProfileToFunctionAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ProfileId: {profileId}, Assignments: {assignments}",
                LogHelpers.Arguments(id, assignments.ToLogString()));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.UpdateProfileToFunctionAssignmentsAsync(
                id,
                assignments,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Gets the assignment information about a function. The result will be a collection of assigned profiles.
    /// </summary>
    /// <param name="id">The function id whose information should be returned..</param>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and a collection of requested assigned profiles has been returned.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If either function or profile could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>
    ///     A task representing the asynchronous read operation. It contains an <see cref="IActionResult" /> depending on
    ///     the success of the operation.
    /// </returns>
    [HttpGet("{id}/profiles", Name = nameof(GetAssignedProfilesOfFunctionAsync))]
    [ProducesResponseType(typeof(ListResponseResult<Member>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignedProfilesOfFunctionAsync(
        [FromRoute] [Required] string id,
        [FromQuery] QueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ProfileId: {profileId}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<Member> members = await _readService.GetAssignedProfiles(id, queryObject, cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(members.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Removes the assignment of a profile to a function.
    /// </summary>
    /// <param name="id">The function id.</param>
    /// <param name="profileId">The profileId which should be unassigned from a function.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">If the request was not processed successfully, because the body of the request was mal-formed.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">
    ///     If the request was not processed successfully, because either role or profile could not be found.
    ///     The response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The deleted function.</returns>
    [HttpDelete("{id}/profiles/{profileId}", Name = nameof(RemoveProfileAssignmentFromFunctionAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> RemoveProfileAssignmentFromFunctionAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "FunctionId: {funcId}, ProfileId: {profileId}",
                LogHelpers.Arguments(id, profileId));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.RemoveProfileFromFunctionAsync(
                profileId,
                ProfileKind.Unknown,
                id,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }
}
