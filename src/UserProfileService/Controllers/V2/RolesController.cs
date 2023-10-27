using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
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
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     Adds, edit or deletes roles for the UserProfileService.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ILogger<RolesController> _logger;
    private readonly IOperationHandler _operationHandler;
    private readonly IReadService _readService;

    public RolesController(
        ILoggerFactory loggerFactory,
        IReadService readService,
        IOperationHandler operationHandler)
    {
        _readService = readService;
        _operationHandler = operationHandler;
        _logger = loggerFactory.CreateLogger<RolesController>();
    }

    /// <summary>
    ///     Returns a list of roles.
    /// </summary>
    /// <param name="queryObject"></param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and a list of roles has been returned.</response>
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
    [HttpGet(Name = nameof(GetRolesAsync))]
    [ProducesResponseType(typeof(ListResponseResult<RoleBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesAsync(
        [FromQuery] QueryObject queryObject = null,
        CancellationToken cancellationToken = default)

    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "QueryObject: {queryObject}",
                LogHelpers.Arguments(queryObject.ToLogString()));
        }

        IPaginatedList<RoleBasic> roles = await _readService.GetRolesAsync<RoleBasic>(
            queryObject,
            cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(roles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Creates a new role.
    /// </summary>
    /// <param name="role">The role parameter that can be set.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not processes successfully, because to body was mal-formed.The response body
    ///     contains an error object with detailed information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns the created Role.</returns>
    [HttpPost(Name = nameof(CreateRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateRoleAsync(
        [FromBody] [Required] CreateRoleRequest role,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "CreateRoleRequest: {createRoleRequest}",
                LogHelpers.Arguments(role.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateRoleAsync(role, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns a specific role.
    /// </summary>
    /// <param name="id">The id of the role to get.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successfully and the body of the response contains the requested role object.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the role has not been found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns the found role searched by id.</returns>
    [HttpGet("{id}", Name = nameof(GetRoleAsync))]
    [ProducesResponseType(typeof(RoleView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {id}",
                LogHelpers.Arguments(id));
        }

        RoleView role = await _readService.GetRoleAsync(id, cancellationToken);
        IActionResult value = ActionResultHelper.ToActionResult(role.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Updates a specific role.
    /// </summary>
    /// <param name="id">The id of the role to update.</param>
    /// <param name="role">The role properties that can be updated.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not processed successfully, because it was mal-formed.The response body
    ///     contains an error object with detailed information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not processed successfully, because the role has not been found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated role.</returns>
    [HttpPut("{id}", Name = nameof(UpdateRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateRoleAsync(
        [FromRoute] [Required] string id,
        [FromBody] [Required] RoleModifiableProperties role,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}, roleModifiableProperties: {roleModifiableProperties} ",
                LogHelpers.Arguments(id, role.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateRoleAsync(id, role, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Deletes a specific role.
    /// </summary>
    /// <param name="id">The id of the role to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not processed successfully, because it was mal-formed.The response body
    ///     contains an error object with detailed information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource. The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated role.</returns>
    [HttpDelete("{id}", Name = nameof(DeleteRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteRoleAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}",
                LogHelpers.Arguments(id));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteRoleAsync(id, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets the assignment information about a role related to a specified profile. The result will be a collection of
    ///     assigned objects.
    /// </summary>
    /// <param name="id">The role id whose information should be returned..</param>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and a collection of requested assigned objects has been returned.</response>
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
    [HttpGet("{id}/profiles", Name = nameof(GetProfilesForRole))]
    [ProducesResponseType(typeof(ListResponseResult<Member>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfilesForRole(
        [FromRoute] [Required] string id,
        [FromQuery] QueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId} ",
                LogHelpers.Arguments(id));
        }

        IPaginatedList<Member> roles = await _readService.GetAssignedProfiles(id, queryObject, cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(roles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Assigns the specified profile to the specified role.
    /// </summary>
    /// <param name="id">The role id which should be assigned to a profile.</param>
    /// <param name="profileId">The profileId which should be assigned to a role.</param>
    /// <param name="conditions">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not processed successfully, because it was mal-formed.The response body
    ///     contains an error object with detailed information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource. The user might not have enough permission.The response
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
    /// <returns>Gets a report, if the objects could be assigned to the profile by role.</returns>
    [HttpPost("{id}/profiles/{profileId}", Name = nameof(AddProfileToRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> AddProfileToRoleAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        [FromBody] RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}, profileId: {profileId}, RangeCondition: {rangeCondition}",
                LogHelpers.Arguments(id, profileId, conditions.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler
                    .AddProfileToRoleAsync(
                        profileId,
                        ProfileKind.Unknown,
                        id,
                        conditions ?? Array.Empty<RangeCondition>(),
                        cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Updates the profile assignments of the specified role.
    /// </summary>
    /// <param name="id">The role id of which the profile assignments should be updated.</param>
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
    ///     If the request was not processed successfully, because either role or profiles could not be
    ///     found.The response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignments of the role.</returns>
    [HttpPut("{id}/profiles", Name = nameof(UpdateProfileToRoleAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateProfileToRoleAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}, BatchAssignmentRequest: {assignments}",
                LogHelpers.Arguments(id, assignments.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateProfileToRoleAssignmentsAsync(
                    id,
                    assignments,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes the profile from the role.
    /// </summary>
    /// <param name="id">The role id which should be unassigned from a profile.</param>
    /// <param name="profileId">The profileId which should be unassigned from a role.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not processed successfully, because it was mal-formed.The response body
    ///     contains an error object with detailed information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource. The user might not have enough permission.The response
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
    /// <returns>Gets a report, if the objects could be assigned to the profile by role.</returns>
    [HttpDelete("{id}/profiles/{profileId}", Name = nameof(RemoveProfileFromRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> RemoveProfileFromRoleAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}, ProfileId: {profileId}",
                LogHelpers.Arguments(id, profileId));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.RemoveProfileFromRoleAsync(
                    profileId,
                    ProfileKind.Unknown,
                    id,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns a list of roles.
    /// </summary>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the list with roles was found and the request processed successfully.</response>
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
    [HttpGet("view", Name = nameof(GetRolesViewAsync))]
    [ProducesResponseType(typeof(ListResponseResult<RoleView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesViewAsync(
        [FromQuery] QueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "QueryObject: {queryObject}",
                LogHelpers.Arguments(queryObject.ToLogString()));
        }

        IPaginatedList<RoleView> roles = await _readService.GetRolesAsync<RoleView>(queryObject, cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(roles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }
}
