using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Maverick.UserProfileService.Models.Abstraction;
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
///     Manages organization profiles that are containers of profiles.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly ILogger<OrganizationsController> _logger;
    private readonly IOperationHandler _operationHandler;
    private readonly IReadService _readService;

    public OrganizationsController(
        ILoggerFactory loggerFactory,
        IReadService readService,
        IOperationHandler operationHandler)
    {
        _readService = readService;
        _operationHandler = operationHandler;
        _logger = loggerFactory.CreateLogger<OrganizationsController>();
    }

    /// <summary>
    ///     Gets a list of organizations.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the response body contains a list of organizations.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of organizations the current user is assigned to.</returns>
    [HttpGet("view", Name = nameof(GetOrganizationsViewAsync))]
    [ProducesResponseType(typeof(ListResponseResult<OrganizationView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationsViewAsync(
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
                RequestedProfileKind.Organization,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets a list of organizations.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the response body contains a list of organizations.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of organizations the current user is assigned to.</returns>
    [HttpGet(Name = nameof(GetOrganizationsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<OrganizationBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationsAsync(
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
                RequestedProfileKind.Organization,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Creates a new organization profile.
    /// </summary>
    /// <param name="organizationProperties">The organization properties that can be set when creating a organization.</param>
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
    /// <returns>Returns the created organization.</returns>
    [HttpPost(Name = nameof(CreateOrganizationAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateOrganizationAsync(
        [FromBody] CreateOrganizationRequest organizationProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "CreateOrganizationRequest: {organizationProperties}",
                LogHelpers.Arguments(organizationProperties.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.CreateOrganizationProfileAsync(
                organizationProperties,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }
    
    /// <summary>
    ///     Returns the root organization profiles that do not have any parents (or: that are not assigned to another
    ///     organization).
    /// </summary>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">
    ///     If the request was processed successfully and the response body contains a list of root
    ///     organizations.If no root object wasn't found an empty list will be return.
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
    [HttpGet("roots", Name = nameof(GetRootOrganizationsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<OrganizationBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRootOrganizationsAsync(
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

        IPaginatedList<IContainerProfile> profiles =
            await _readService.GetRootProfilesAsync<GroupView, OrganizationView>(
                RequestedProfileKind.Organization,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns the specific organizations profile.
    /// </summary>
    /// <param name="id">The id of the organization to get.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the organization profile is in the body of the response.</response>
    /// <response code="400">If the request was not valid.An error object has been returned that contains detailed information.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified organization could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Return the specified organization.</returns>
    [HttpGet("{id}", Name = nameof(GetOrganizationAsync))]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}",
                LogHelpers.Arguments(id));
        }

        var profile = await _readService.GetProfileAsync<Organization>(
            id,
            RequestedProfileKind.Organization,
            cancellationToken: cancellationToken);

        IActionResult value = ActionResultHelper.ToActionResult(profile.ResolveUrlProperties(ControllerContext));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Updates a specific organization.
    /// </summary>
    /// <param name="id">The id of the organization to be updated.</param>
    /// <param name="organizationProperties">The properties from the organization that can be updated.</param>
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
    /// <response code="404">If the specified organization could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated organization.</returns>
    [HttpPut("{id}", Name = nameof(UpdateOrganizationProfileAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateOrganizationProfileAsync(
        [FromRoute] [Required] string id,
        [FromBody] OrganizationModifiableProperties organizationProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}, OrganizationModifiableProperties: {organizationProperties}",
                LogHelpers.Arguments(id, organizationProperties.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.UpdateOrganizationProfileAsync(
                id,
                organizationProperties,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }
    
    /// <summary>
    ///     Deletes a specific organization.
    /// </summary>
    /// <param name="id">The id of the organization has to be deleted.</param>
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
    /// <returns>The deleted organization.</returns>
    [HttpDelete("{id}", Name = nameof(DeleteOrganizationAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteOrganizationAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}",
                LogHelpers.Arguments(id));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.DeleteOrganizationAsync(id, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns a list of users or organizations that are member of the specified organization. (Only Basic)
    /// </summary>
    /// <param name="id">The id of the organization whose members to be get.</param>
    /// <param name="profileKind">The id of the organization whose members to be get.</param>
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
    /// <response code="404">If the request was not successful, because the specified organization could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns user and organizations that are part of the organization.</returns>
    [HttpGet("{id}/children", Name = nameof(GetChildrenOfOrganizationAsync))]
    [ProducesResponseType(typeof(ListResponseResult<IProfile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildrenOfOrganizationAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject =
            default,
        [FromQuery] RequestedProfileKind profileKind = RequestedProfileKind.All,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}, queryObject: {queryObject}, RequestedProfileKind: {profileKind}",
                LogHelpers.Arguments(id, queryObject.ToLogString(), profileKind.ToLogString()));
        }

        IPaginatedList<IProfile> profiles = await _readService
            .GetChildrenOfProfileAsync<ConditionalUser, ConditionalGroup, ConditionalOrganization>(
                id,
                ProfileContainerType.Organization,
                profileKind,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns a list of users or organizations that are member of the specified organization.
    /// </summary>
    /// <param name="id">The id of the organization the user to be returned is part of.</param>
    /// <param name="queryObject">Contains options and filter for Collections.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request processed successfully.A list of parent organizations.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified organization could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns user and organizations that are part of the organization.</returns>
    [HttpGet("{id}/parents", Name = nameof(GetParentsOfOrganizationProfileAsync))]
    public async Task<IActionResult> GetParentsOfOrganizationProfileAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject =
            default,
        CancellationToken cancellationToken =
            default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}, queryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<IContainerProfile> profiles =
            await _readService.GetParentsOfProfileAsync<ConditionalGroup, ConditionalOrganization>(
                id,
                RequestedProfileKind.Organization,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Adds an existing profile to a specified organization.
    /// </summary>
    /// <param name="id">The id of the organization the specified user should be added to.</param>
    /// <param name="profileId">The id of the user or organization profile to be added as member to specified organization.</param>
    /// <param name="conditions">
    ///     Condition when the assignment is valid. Applies only to users assigned to organizations.
    ///     Otherwise ignored.
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
    ///     If either the organization or the profile could not be found.An error object has been returned
    ///     that contains detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignment that has been made from the profiles to the organization.</returns>
    [HttpPut("{id}/profiles/{profileId}", Name = nameof(AssignProfileToOrganizationAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> AssignProfileToOrganizationAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        [FromBody] RangeCondition[] conditions,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}, profileId: {profileId}, RangeCondition: {conditions}",
                LogHelpers.Arguments(id, profileId, conditions.ToLogString()));
        }

        var organization = new ProfileIdent(id, ProfileKind.Organization);
        var profiles = new[] { new ProfileIdent(profileId, ProfileKind.Unknown) };

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.AssignProfilesToContainerProfileAsync(
                profiles,
                organization,
                conditions ?? Array.Empty<RangeCondition>(),
                cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Updates the assignments of profiles to a specified organization.
    /// </summary>
    /// <param name="id">The id of the organization the specified profiles should be added to or removed from.</param>
    /// <param name="request">A batch request for organizations. Multiple organization can be updated.</param>
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
    ///     If either the organization or the profiles could not be found.An error object has been returned
    ///     that contains detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignments that has been updated from the profiles to the organization.</returns>
    [HttpPut("{id}/profiles", Name = nameof(UpdateProfilesToOrganizationAssignmentsAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateProfilesToOrganizationAssignmentsAsync(
        [FromRoute] [Required] string id,
        [FromBody] BatchAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {organizationId}, BatchAssignmentRequest: {request}",
                LogHelpers.Arguments(id, request.ToLogString()));
        }

        var organization = new ProfileIdent(id, ProfileKind.Organization);

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.UpdateProfilesToContainerProfileAssignmentsAsync(
                organization,
                request,
                cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes the organization membership of specified user of specified organization.
    /// </summary>
    /// <param name="id">The id of the organization whose members should be changed.</param>
    /// <param name="profileId">The id of the user or organization profile to be removed from organization.</param>
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
    ///     If either the organization or the profile could not be found.An error object has been returned
    ///     that contains detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The assignment that has been unassigned from the organization.</returns>
    [HttpDelete("{id}/profiles/{profileId}", Name = nameof(RemoveProfileAssignmentFromOrganizationAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> RemoveProfileAssignmentFromOrganizationAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string profileId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {id}, ProfileId: {profileId}",
                LogHelpers.Arguments(id, profileId));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () => _operationHandler.RemoveProfileAssignmentsFromContainerProfileAsync(
                new[] { new ProfileIdent(profileId, ProfileKind.Unknown) },
                new ProfileIdent(id, ProfileKind.Organization),
                cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets the assignment information about a organization related to a specified role. The result will be a collection
    ///     of assigned objects.
    /// </summary>
    /// <param name="id">The id of the organization whose information should be returned.</param>
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
    [HttpGet("{id}/roles", Name = nameof(GetRolesOfOrganizationAsync))]
    [ProducesResponseType(typeof(ListResponseResult<RoleBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesOfOrganizationAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject =
            default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {id}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<LinkedRoleObject> roles =
            await _readService.GetRolesOfProfileAsync(id, queryObject, cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(roles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets the assignment information about a organization related to a specified function. The result will be a
    ///     collection of assigned objects.
    /// </summary>
    /// <param name="id">The id of the organization whose information should be returned.</param>
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
    [HttpGet("{id}/functions", Name = nameof(GetFunctionsOfOrganizationAsync))]
    [ProducesResponseType(typeof(ListResponseResult<FunctionBasic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFunctionsOfOrganizationAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject =
            default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "OrganizationId: {id}, QueryObject: {queryObject}",
                LogHelpers.Arguments(id, queryObject.ToLogString()));
        }

        IPaginatedList<LinkedFunctionObject> functions =
            await _readService.GetFunctionsOfProfileAsync(id, false, queryObject, cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(functions.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }
}
