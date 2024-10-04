using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Asp.Versioning;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Api.Common.Abstractions;
using UserProfileService.Api.Common.Attributes;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Api.Common.Extensions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     Manages the user profiles stored in the UserProfileService.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IDeputyService _deputyService;
    private readonly ILogger<UsersController> _logger;
    private readonly IOperationHandler _operationHandler;
    private readonly IReadService _readService;
    private readonly IUserContextStore _userContextStore;

    public UsersController(
        ILoggerFactory loggerFactory,
        IReadService readService,
        IOperationHandler operationHandler,
        IUserContextStore userContextStore,
        IDeputyService deputyService)
    {
        _readService = readService;
        _operationHandler = operationHandler;
        _userContextStore = userContextStore;
        _deputyService = deputyService;
        _logger = loggerFactory.CreateLogger<UsersController>();
    }

    /// <summary>
    ///     Creates a new user.
    /// </summary>
    /// <param name="userProperties">The user properties that can be set. </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The created user.</returns>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">If the id or the user properties are not set.</response>
    /// <response code="500">If a server-side error occurred and a user could not be created.</response>
    [HttpPost(Name = nameof(CreateUserAsync))]
    [Consumes("application/json")]
    [Produces("application/json", "plain/text")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateUserAsync(
        [FromBody] [Required] CreateUserRequest userProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "CreateUserRequest: {createUserRequest},",
                LogHelpers.Arguments(userProperties.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateUserProfileAsync(userProperties, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Deletes a single user. All assignments will also be removed.
    /// </summary>
    /// <param name="id">The id of the that should be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">If the id is not set.</response>
    /// <response code="500">If a server-side error occurred and a user could not be updated.</response>
    /// <returns>The deleted user profile.</returns>
    [HttpDelete("{id}", Name = nameof(DeleteUser))]
    [Consumes("application/json")]
    [Produces("application/json", "plain/text")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] [Required] [StringLength(200, ErrorMessage = "The Id values cannot exceed 200 characters.")]
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId},",
                LogHelpers.Arguments(id));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteUserAsync(id, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns a list of users.
    /// </summary>
    /// <param name="version">Defined api version.</param>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">
    ///     If the response executed correctly and the list of users has been returned.If none was found, the
    ///     result will contain an empty list.
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
    /// <returns>A list of groups the current user is assigned to.</returns>
    /// <returns>Returns a list of users.</returns>
    [HttpGet(Name = nameof(GetAllUsersAsync))]
    [ProducesResponseType(typeof(ListResponseResult<UserBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsersAsync(
        ApiVersion
            version, // is used to get the correct routing for custom property links, filter links, etc. It won't be part of OpenAPI definition file.
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Pagination settings: offset {offset} and limit {limit}.",
                LogHelpers.Arguments(queryObject?.Offset, queryObject?.Limit));
        }

        IPaginatedList<IProfile> profiles =
            await _readService.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                RequestedProfileKind.User,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }
    
    /// <summary>
    ///     Gets the profile of the current user.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the profile object is in the response body.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Profile of the current user.</returns>
    [Authorize]
    [HttpGet("me", Name = nameof(GetOwnProfileAsync))]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOwnProfileAsync(CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        string currentUserId = _userContextStore.GetIdOfCurrentUser();

        List<IProfile> profiles =
            await _readService.GetProfilesByExternalOrInternalIdAsync<User, Group, Organization>(
                currentUserId,
                cancellationToken: cancellationToken);

        IProfile profile = profiles.FirstOrDefault();

        if (profiles.Count == 0
            || profile is not
            {
                Kind: ProfileKind.User
            })
        {
            return _logger.ExitMethod(NotFound($"The user with the id {currentUserId} could not be found!"));
        }

        IActionResult result = ActionResultHelper.ToActionResult(profile.ToPropertiesChangeDictionary());

        return result;
    }

    /// <summary>
    ///     Updates the modifiable properties of the current user profile.
    /// </summary>
    /// <param name="userProperties">The user properties that can be modified.</param>
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
    /// <returns></returns>
    [HttpPut("me", Name = nameof(UpdateOwnProfileAsync))]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateOwnProfileAsync(
        [FromBody] UserModifiableProperties userProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserModifiableProperties: {userModifiableProperties},",
                LogHelpers.Arguments(userProperties.ToLogString()));
        }

        string currentUserId = _userContextStore.GetIdOfCurrentUser();

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateUserProfileAsync(currentUserId, userProperties, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns the specific user profile.
    /// </summary>
    /// <param name="id">The id of the user to get.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request processed successfully and the modified user profile has been returned..</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the specified user could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The requested user.</returns>
    [HttpGet("{id}", Name = nameof(GetUserProfileAsync))]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserProfileAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId},",
                LogHelpers.Arguments(id));
        }

        var profile =
            await _readService.GetProfileAsync<User>(
                id,
                RequestedProfileKind.User,
                cancellationToken: cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profile.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns the deputy for the specific user profile.
    /// </summary>
    /// <param name="id">The id of the user to get the deputy for.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request processed successfully and the deputy user profile has been returned.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the specified user could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The requested deputy user.</returns>
    [HttpGet("{id}/deputy", Name = nameof(GetUserProfileDeputyAsync))]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserProfileDeputyAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId},",
                LogHelpers.Arguments(id));
        }

        var profile =
            await _deputyService.GetDeputyOfProfileAsync<User>(
                id,
                RequestedProfileKind.User,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profile.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Updates the modifiable properties of the specific user profile.
    /// </summary>
    /// <param name="id">The id of the user to update.</param>
    /// <param name="userModifiableProperties">The user properties that can be modified.</param>
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
    /// <response code="404">If the specified user could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The modified user.</returns>
    [HttpPut("{id}", Name = nameof(UpdateUserProfileAsync))]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateUserProfileAsync(
        [FromRoute] [Required] string id,
        [FromBody] [Required] UserModifiableProperties userModifiableProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, UserModifiableProperties: {userModifiableProperties}",
                LogHelpers.Arguments(id, userModifiableProperties.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateUserProfileAsync(
                    id,
                    userModifiableProperties,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets the assignment information about a user profile related to a specified role. The result will be a collection
    ///     of assigned objects.
    /// </summary>
    /// <param name="id">The id of the user profile whose information should be returned.</param>
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
    [HttpGet("{id}/roles", Name = nameof(GetAssignedObjectsOfUserByRoleAsync))]
    [ProducesResponseType(typeof(ListResponseResult<LinkedRoleObject>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignedObjectsOfUserByRoleAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}",
                LogHelpers.Arguments(id));
        }

        IPaginatedList<LinkedRoleObject> roles =
            await _readService.GetRolesOfProfileAsync(id, queryObject, cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(roles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Updates the role assignments of the specified user.
    /// </summary>
    /// <param name="id">The user id of which the role assignments should be updated.</param>
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
    ///     If the request was not processed successfully, because either user or roles could not be found.The
    ///     response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The role assignments of the user.</returns>
    [HttpPost("{id}/roles", Name = nameof(UpdateRoleToUserAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateRoleToUserAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, BatchAssignmentRequest: {batchAssignmentRequest}",
                LogHelpers.Arguments(id, assignments.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateRoleToUserAssignmentsAsync(
                    id,
                    assignments,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets the assignment information about a user profile related to a specified function. The result will be a
    ///     collection of assigned objects.
    /// </summary>
    /// <param name="id">The id of the user profile whose information should be returned..</param>
    /// <param name="returnFunctionsRecursively">
    ///     States that the direct assigned functions should be returned, if false and recursively
    ///     assigned functions (for example from a group) otherwise. If the functions returned recursively only active
    ///     assignments will be returned.
    /// </param>
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
    [HttpGet("{id}/functions", Name = nameof(GetAssignedObjectsOfUserByFunctionAsync))]
    [ProducesResponseType(typeof(ListResponseResult<LinkedFunctionObject>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignedObjectsOfUserByFunctionAsync(
        [FromRoute] [Required] string id,
        [FromQuery] bool returnFunctionsRecursively = false,
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<LinkedFunctionObject> functions =
            await _readService.GetFunctionsOfProfileAsync(
                id,
                returnFunctionsRecursively,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(functions.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Updates the function assignments of the specified user.
    /// </summary>
    /// <param name="id">The user id of which the function assignments should be updated.</param>
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
    ///     If the request was not processed successfully, because either user or functions could not be
    ///     found.The response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The function assignments of the user.</returns>
    [HttpPut("{id}/functions", Name = nameof(UpdateFunctionToUserAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateFunctionToUserAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, BatchAssignmentRequest: {batchAssignmentRequest}",
                LogHelpers.Arguments(id, assignments.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateFunctionToUserAssignmentsAsync(
                    id,
                    assignments,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Updates the container profile assignments of the specified user.
    /// </summary>
    /// <param name="id">The user id of which the container profile assignments should be updated.</param>
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
    ///     If the request was not processed successfully, because either user or profiles could not be
    ///     found.The response body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The container profile assignments of the user.</returns>
    [HttpPost("{id}/profiles", Name = nameof(UpdateContainerProfileToUserAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateContainerProfileToUserAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, BatchAssignmentRequest: {batchAssignmentRequest}",
                LogHelpers.Arguments(id, assignments.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateContainerProfileToUserAssignmentsAsync(
                    id,
                    assignments,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets a list of <see cref="UserView" />.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the response body contains a list of users.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    /// 
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of groups the current user is assigned to.</returns>
    [HttpGet("view", Name = nameof(GetUsersViewAsync))]
    [ProducesResponseType(typeof(ListResponseResult<UserView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersViewAsync(
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

        IPaginatedList<IProfile> functions =
            await _readService.GetProfilesAsync<UserView, GroupView, OrganizationView>(
                RequestedProfileKind.User,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(functions.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns the merged configuration for a user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="configKey">The configuration identifier.</param>
    /// <param name="includeInherited">Whether to include inherited configs or not.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpGet("{userId}/config/{configKey}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserConfig(
        string userId,
        string configKey,
        bool includeInherited = true,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, ConfigKey: {configKey}, includeInherited {includeInherited}",
                LogHelpers.Arguments(userId, configKey, includeInherited));
        }

        JObject config =
            await _readService.GetSettingsOfProfileAsync(
                userId,
                ProfileKind.User,
                configKey,
                includeInherited,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(config);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Adds or replaces the configuration of targeted user that is classified with provided configuration key.
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="validJsonConfig">The user configuration</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpPut("{userId}/config/{configKey}")]
    [Consumes(MediaTypeNames.Text.Plain)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> SetUserConfig(
        string userId,
        string configKey,
        [FromBody] [ModelBinder(typeof(PlainModelBinder))] string validJsonConfig,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, ConfigKey: {configKey}",
                LogHelpers.Arguments(userId, configKey));
        }

        try
        {
            JObject jsonConfig = JObject.Parse(validJsonConfig);

            IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
                () => _operationHandler.SetProfileConfiguration(
                    userId,
                    ProfileKind.User,
                    configKey,
                    jsonConfig,
                    cancellationToken),
                _logger);

            return _logger.ExitMethod(result);
        }
        catch (JsonException ex)
        {
            _logger.LogWarnMessage(ex, "The given config is not a valid json.", LogHelpers.Arguments());

            throw new NotValidException("The given config is not a valid json.", ex);
        }
    }

    /// <summary>
    ///     Adds or replaces the configuration of targeted user that is classified with provided configuration key.
    /// </summary>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="configRequest">The user configuration and user ids.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpPut("config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> SetUsersConfig(
        string configKey,
        [FromBody] BatchConfigSettingsRequest configRequest,
        CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ConfigRequest: {configRequest}",
                LogHelpers.Arguments(configRequest.ToLogString()));
        }

        try
        {
            _logger.EnterMethod();

            IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
                () => _operationHandler.SetProfilesConfiguration(
                    configKey,
                    configRequest,
                    ProfileKind.User,
                    cancellationToken),
                _logger);

            return _logger.ExitMethod(result);
        }
        catch (JsonException ex)
        {
            _logger.LogInfoMessage(
                "The given config is not a valid json. Error-Message: {errorMessage}. Inner message: {innerMessage}",
                LogHelpers.Arguments(ex.Message, ex.InnerException?.Message));

            throw new NotValidException("The given config is not a valid json.", ex);
        }
    }

    /// <summary>
    ///     Updates parts of the configuration of targeted user that is classified with provided configuration key.
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="configPatchDocument">A valid JsonPatchDocument to update single variables of a config</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpPatch("{userId}/config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateUserConfig(
        string userId,
        string configKey,
        [FromBody] JsonPatchDocument configPatchDocument,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ConfigPatchDocument: {configPatchDocument}",
                LogHelpers.Arguments(configPatchDocument.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateProfileConfiguration(
                    userId,
                    ProfileKind.User,
                    configKey,
                    configPatchDocument,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes a config from a user
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpDelete("{userId}/config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteUserConfig(
        string userId,
        string configKey,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, configKey: {configKey}",
                LogHelpers.Arguments(userId, configKey));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.RemoveProfileConfiguration(
                    userId,
                    ProfileKind.User,
                    configKey,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets ids of all associated profiles
    /// </summary>
    /// <remarks>
    ///     Gets a list of ids of all associated profiles: This can be groups or,functions, direct or indirect related to the
    ///     requested profile.
    ///     The resulting collection will contain <see cref="ObjectIdent" /> instances representing Ids and type of related
    ///     objects.
    /// </remarks>
    /// <param name="userId">
    ///     The id of the user whose associated profiles should be returned.
    /// </param>
    /// <param name="includeInactiveAssignments">
    ///     A boolean flag indicating whether inactive assigned profiles should be returned too.
    ///     If <c>true</c>, inactive assignments will be considered as well, otherwise, they will be skipped.
    ///     Default value: <c>false</c>
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">
    ///     If the request was processed successfully and the response body contains a list of associated
    ///     profile ids.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified profile could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="ObjectIdent" />s referring to
    ///     associated profiles.
    /// </returns>
    [HttpGet("{userId}/associated", Name = nameof(GetAssociatedIds))]
    [ProducesResponseType(typeof(List<ObjectIdent>), StatusCodes.Status200OK)]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetAssociatedIds(
        string userId,
        bool includeInactiveAssignments = false,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ProfileId: {profileId}, includeInactiveAssignments: {includeInactiveAssignments}",
                LogHelpers.Arguments(
                    userId.ToLogString(),
                    includeInactiveAssignments));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("The profileId is null, empty or contains only whitespaces.");
        }

        IList<ObjectIdent> objectIdentList =
            await _readService.GetAllAssignedIdsOfUserAsync(
                userId,
                includeInactiveAssignments,
                cancellationToken);

        return _logger.ExitMethod(ActionResultHelper.ToActionResult(objectIdentList));
    }
}
