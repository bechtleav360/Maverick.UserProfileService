using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Api.Common.Abstractions;
using UserProfileService.Api.Common.Attributes;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Api.Common.Extensions;
using UserProfileService.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Configuration;
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     Manages group profiles that are containers of profiles.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly ILogger<GroupsController> _logger;
    private readonly IOperationHandler _operationHandler;
    private readonly IReadService _readService;

    public GroupsController(
        ILoggerFactory loggerFactory,
        IReadService readService,
        IOperationHandler operationHandler)
    {
        _readService = readService;
        _operationHandler = operationHandler;
        _logger = loggerFactory.CreateLogger<GroupsController>();
    }

    /// <summary>
    ///     Gets a list of groups.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the response body contains a list of groups.</response>
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
    [HttpGet("view", Name = nameof(GetGroupsViewAsync))]
    [ProducesResponseType(typeof(ListResponseResult<GroupView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupsViewAsync(
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

        IPaginatedList<IProfile> profiles =
            await _readService.GetProfilesAsync<UserView, GroupView, OrganizationView>(
                RequestedProfileKind.Group,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Gets a list of groups.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the response body contains a list of groups.</response>
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
    [HttpGet(Name = nameof(GetGroupsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<GroupBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupsAsync(
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

        IPaginatedList<IProfile> profiles =
            await _readService.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                RequestedProfileKind.Group,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Creates a new group profile.
    /// </summary>
    /// <param name="groupProperties">The group properties that can be set when creating a group.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request could not be processed and an object is in the body of the response that contains
    ///     detailed information about the error.
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
    /// <returns>Returns the created group.</returns>
    [HttpPost(Name = nameof(CreateGroupAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateGroupAsync(
        [FromBody] CreateGroupRequest groupProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "CreateGroupRequest: {createGroupRequest}",
                LogHelpers.Arguments(groupProperties.ToLogString()));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.CreateGroupProfileAsync(
                groupProperties,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }
    
    /// <summary>
    ///     Returns the root group profiles that do not have any parents (or: that are not assigned to another group).
    /// </summary>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">
    ///     If the request was processed successfully and the response body contains a list of root groups.If
    ///     no root object wasn't found an empty list will be return.
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
    /// <returns></returns>
    [HttpGet("roots", Name = nameof(GetRootGroupsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<GroupBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRootGroupsAsync(
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "SearchObject: {searchObject}",
                LogHelpers.Arguments(queryObject.ToLogString()));
        }

        IPaginatedList<IContainerProfile> profiles =
            await _readService.GetRootProfilesAsync<GroupBasic, OrganizationBasic>(
                RequestedProfileKind.Group,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns the specific groups profile.
    /// </summary>
    /// <param name="id">The id of the group to get.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the group profile is in the body of the response.</response>
    /// <response code="400">If the request was not valid.An error object has been returned that contains detailed information.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified group could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Return the specified group.</returns>
    [HttpGet("{id}", Name = nameof(GetGroupAsync))]
    [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}",
                LogHelpers.Arguments(id));
        }

        var profile =
            await _readService.GetProfileAsync<Group>(
                id,
                RequestedProfileKind.Group,
                cancellationToken: cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profile.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns the specific groups profile.
    /// </summary>
    /// <param name="id">The id of the group to get.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the group profile is in the body of the response.</response>
    /// <response code="400">If the request was not valid.An error object has been returned that contains detailed information.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified group could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Return the specified group.</returns>
    [HttpGet("{id}/view", Name = nameof(GetGroupViewAsync))]
    [ProducesResponseType(typeof(GroupView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupViewAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}",
                LogHelpers.Arguments(id));
        }

        var profile =
            await _readService.GetProfileAsync<GroupView>(
                id,
                RequestedProfileKind.Group,
                cancellationToken: cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profile.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Updates a specific group.
    /// </summary>
    /// <param name="id">The id of the group to be updated.</param>
    /// <param name="groupProperties">The properties from the group that can be updated.</param>
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
    /// <response code="404">If the specified group could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated group.</returns>
    [HttpPut("{id}", Name = nameof(UpdateGroupProfileAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateGroupProfileAsync(
        [FromRoute] [Required] string id,
        [FromBody] GroupModifiableProperties groupProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, GroupModifiableProperties: {groupProperties}",
                LogHelpers.Arguments(id, groupProperties.ToLogString()));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.UpdateGroupProfileAsync(
                id,
                groupProperties,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Deletes a specific group.
    /// </summary>
    /// <param name="id">The id of the group has to be deleted.</param>
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
    /// <returns>The deleted group.</returns>
    [HttpDelete("{id}", Name = nameof(DeleteGroupAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteGroupAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}",
                LogHelpers.Arguments(id));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.DeleteGroupAsync(id, cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns a list of users or groups that are member of the specified group. (Only Basic)
    /// </summary>
    /// <param name="id">The id of the group whose members to be get.</param>
    /// <param name="profileKind">Which profile kind of children should be returned.</param>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was processed successfully and the response body contains a list of child profiles.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns user and groups that are part of the group.</returns>
    [HttpGet("{id}/children", Name = nameof(GetChildrenOfGroupAsync))]
    [ProducesResponseType(typeof(ListResponseResult<IProfile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildrenOfGroupAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject = default,
        [FromQuery] RequestedProfileKind profileKind = RequestedProfileKind.All,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, QueryObject: {queryObject}, ProfileKind: {profileKind}",
                LogHelpers.Arguments(id, queryObject.ToLogString(), profileKind));
        }

        IPaginatedList<IProfile> profiles =
            await _readService
                .GetChildrenOfProfileAsync<ConditionalUser, ConditionalGroup, ConditionalOrganization>(
                    id,
                    ProfileContainerType.Group,
                    profileKind,
                    queryObject,
                    cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns a list of users or groups that are member of the specified group.
    /// </summary>
    /// <param name="id">The id of the group the user to be returned is part of.</param>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request processed successfully.A list of parent groups.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified group could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns user and groups that are part of the group.</returns>
    [HttpGet("{id}/parents", Name = nameof(GetParentsOfGroupProfileAsync))]
    public async Task<IActionResult> GetParentsOfGroupProfileAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<IContainerProfile> profiles =
            await _readService.GetParentsOfProfileAsync<ConditionalGroup, ConditionalOrganization>(
                id,
                RequestedProfileKind.Group,
                queryObject,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Adds an existing profile to a specified group.
    /// </summary>
    /// <param name="id">The id of the group the specified user should be added to.</param>
    /// <param name="profileId">The id of the user or group profile to be added as member to specified group.</param>
    /// <param name="conditions">
    ///     Condition when the assignment is valid. Applies only to users assigned to
    ///     groups/organizations. Otherwise ignored.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not successful. An error object has been returned that contains detailed
    ///     information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">
    ///     If either the group or the profile could not be found.An error object has been returned that
    ///     contains detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignment that has been made from the profiles to the group.</returns>
    [HttpPut("{id}/profiles/{profileId}", Name = nameof(AssignProfileToGroupAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> AssignProfileToGroupAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        [FromBody] RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, ProfileId: {profileId}, RangeConditions: {rangeConditions}",
                LogHelpers.Arguments(id, profileId, conditions.ToLogString()));
        }

        var group = new ProfileIdent(id, ProfileKind.Group);
        var profiles = new[] { new ProfileIdent(profileId, ProfileKind.Unknown) };

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.AssignProfilesToContainerProfileAsync(
                profiles,
                group,
                conditions ?? Array.Empty<RangeCondition>(),
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Updates the assignments of profiles to a specified group.
    /// </summary>
    /// <param name="id">The id of the group the specified profiles should be added to or removed from.</param>
    /// <param name="request">Assignments of group to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the request was not successful. An error object has been returned that contains detailed
    ///     information.
    /// </response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">
    ///     If either the group or the profiles could not be found.An error object has been returned that
    ///     contains detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignments that has been updated from the profiles to the group.</returns>
    [HttpPut("{id}/profiles", Name = nameof(UpdateProfilesToGroupAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateProfilesToGroupAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, BatchAssignmentRequest: {batchAssignmentRequest}",
                LogHelpers.Arguments(id, request.ToLogString()));
        }

        var group = new ProfileIdent(id, ProfileKind.Group);

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.UpdateProfilesToContainerProfileAssignmentsAsync(
                group,
                request,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Removes the group membership of specified user of specified group.
    /// </summary>
    /// <param name="id">The id of the group whose members should be changed.</param>
    /// <param name="profileId">The id of the user or group profile to be removed from group.</param>
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
    /// <response code="404">
    ///     If either the group or the profile could not be found.An error object has been returned that
    ///     contains detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignment that has been unassigned from the group.</returns>
    [HttpDelete("{id}/profiles/{profileId}", Name = nameof(RemoveProfileMembershipOfGroupAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> RemoveProfileMembershipOfGroupAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, ProfileId: {profileId}",
                LogHelpers.Arguments(id, profileId));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.RemoveProfileAssignmentsFromContainerProfileAsync(
                new[] { new ProfileIdent(profileId, ProfileKind.Unknown) },
                new ProfileIdent(id, ProfileKind.Group),
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Gets the assignment information about a group related to a specified role. The result will be a collection of
    ///     assigned objects.
    /// </summary>
    /// <param name="id">The id of the group whose information should be returned.</param>
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
    [HttpGet("{id}/roles", Name = nameof(GetRolesOfGroupAsync))]
    [ProducesResponseType(typeof(ListResponseResult<RoleBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesOfGroupAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<LinkedRoleObject> roles =
            await _readService.GetRolesOfProfileAsync(id, queryObject, cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(roles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Gets the assignment information about a group related to a specified function. The result will be a collection of
    ///     assigned objects.
    /// </summary>
    /// <param name="id">The id of the group whose information should be returned.</param>
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
    [HttpGet("{id}/functions", Name = nameof(GetFunctionsOfGroupAsync))]
    [ProducesResponseType(typeof(ListResponseResult<FunctionBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFunctionsOfGroupAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<LinkedFunctionObject> functions =
            await _readService.GetFunctionsOfProfileAsync(id, false, queryObject, cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(functions.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Returns the merged configuration for a group.
    /// </summary>
    /// <param name="groupId">The group id.</param>
    /// <param name="configKey">The configuration identifier.</param>
    /// <param name="includeInherited">Whether to include inherited configs or not.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration.</returns>
    [HttpGet("{groupId}/config/{configKey}")]
    [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupConfig(
        string groupId,
        string configKey,
        bool includeInherited = true,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, ConfigKey: {configKey}, includeInherited: {includeInherited} ",
                LogHelpers.Arguments(groupId, configKey, includeInherited));
        }

        JObject config =
            await _readService.GetSettingsOfProfileAsync(
                groupId,
                ProfileKind.Group,
                configKey,
                includeInherited,
                cancellationToken);

        IActionResult result =
            ActionResultHelper.ToActionResult(config);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Adds or replaces the configuration of targeted group that is classified with provided configuration key.
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="validJsonConfig">The group configuration</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpPut("{groupId}/config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> SetGroupConfig(
        string groupId,
        string configKey,
        [FromBody] [ModelBinder(typeof(PlainModelBinder))] string validJsonConfig,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, ConfigKey: {configKey}, ValidJsonConfig: {validJsonConfig} ",
                LogHelpers.Arguments(groupId, configKey, validJsonConfig));
        }

        try
        {
            JObject jsonConfig = JObject.Parse(validJsonConfig);

            IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
                () => _operationHandler.SetProfileConfiguration(
                    groupId,
                    ProfileKind.Group,
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
    ///     Adds or replaces the configuration of targeted group that is classified with provided configuration key.
    /// </summary>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="configRequest">The group configuration and group ids.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpPut("config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> SetGroupsConfig(
        string configKey,
        [FromBody] BatchConfigSettingsRequest configRequest,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ConfigKey: {configKey}, configRequest: {configRequest}",
                LogHelpers.Arguments(configKey, configRequest.ToLogString()));
        }

        try
        {
            IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
                () => _operationHandler.SetProfilesConfiguration(
                    configKey,
                    configRequest,
                    ProfileKind.Group,
                    cancellationToken),
                _logger);

            return _logger.ExitMethod(result);
        }
        catch (JsonException ex)
        {
            _logger.LogInfoMessage(
                "The given config is not a valid json. Error: {errorMessage}",
                LogHelpers.Arguments(ex.Message));

            throw new NotValidException("The given config is not a valid json.", ex);
        }
    }

    /// <summary>
    ///     Updates parts of the configuration of targeted group that is classified with provided configuration key.
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="configPatchDocument">A valid JsonPatchDocument to update single variables of a config</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpPatch("{groupId}/config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateGroupConfig(
        string groupId,
        string configKey,
        [FromBody] JsonPatchDocument configPatchDocument,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, configRequest: {configRequest}",
                LogHelpers.Arguments(groupId, configKey));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.UpdateProfileConfiguration(
                groupId,
                ProfileKind.Group,
                configKey,
                configPatchDocument,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }

    /// <summary>
    ///     Removes a config from a group
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="configKey">The configuration identifier</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The merged json configuration</returns>
    [HttpDelete("{groupId}/config/{configKey}")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteGroupConfig(
        string groupId,
        string configKey,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, configKey: {configKey}",
                LogHelpers.Arguments(groupId, configKey));
        }

        IActionResult result = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.RemoveProfileConfiguration(
                groupId,
                ProfileKind.Group,
                configKey,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(result);
    }
}
