using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Asp.Versioning;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Api.Common.Extensions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     Controller to get combined group and user views.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly ILogger<ProfilesController> _logger;
    private readonly IReadService _readService;

    public ProfilesController(
        ILoggerFactory loggerFactory,
        IReadService readService)
    {
        _readService = readService;
        _logger = loggerFactory.CreateLogger<ProfilesController>();
    }

    /// <summary>
    ///     Gets a list of <see cref="IProfile" />.
    /// </summary>
    /// <param name="profileKind">Which profile kind of children should be returned.</param>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the response body contains a list of users and groups.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of profiles.</returns>
    [HttpGet("view", Name = nameof(GetProfilesViewAsync))]
    [ProducesResponseType(typeof(ListResponseResult<IProfile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfilesViewAsync(
        [FromQuery] RequestedProfileKind profileKind = RequestedProfileKind.All,
        [FromQuery] AssignmentQueryObject queryObject = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "QueryObject: {queryObject}, profileKind: {profileKind}",
                LogHelpers.Arguments(queryObject.ToLogString(), profileKind.ToLogString()));
        }

        IPaginatedList<IProfile> profiles =
            await _readService.GetProfilesAsync<UserBasic, GroupView, OrganizationView>(
                profileKind,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns a list of profiles that are member of the specified container profile. (Only Basic)
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
    /// <response code="404">If the request was not successful, because the specified container profile could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns profile that are part of the container profile.</returns>
    [HttpGet("{id}/children", Name = nameof(GetChildrenOfContainerProfileAsync))]
    [ProducesResponseType(typeof(ListResponseResult<IProfile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildrenOfContainerProfileAsync(
        [FromRoute] [Required] string id,
        [FromQuery] AssignmentQueryObject queryObject = default,
        [FromQuery] RequestedProfileKind profileKind = RequestedProfileKind.All,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Id: {id}, QueryObject: {queryObject}, profileKind: {profileKind}",
                LogHelpers.Arguments(id, queryObject.ToLogString(), profileKind.ToLogString()));
        }

        IPaginatedList<IProfile> profiles = await _readService
            .GetChildrenOfProfileAsync<ConditionalUser, ConditionalGroup, ConditionalOrganization>(
                id,
                ProfileContainerType.Group,
                profileKind,
                queryObject,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(profiles.ToListResponseResult(HttpContext, queryObject));

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Return a <see cref="IProfile" /> by using the external id property. The profile can be an
    ///     <see cref="Organization" />,
    ///     <see cref="Group" /> or an <see cref="User" /> object.
    /// </summary>
    /// <param name="profileId">
    ///     The profileId identifies the profile by its id, if the
    ///     <paramref name="allowExternalIds" /> uses the default value true.
    /// </param>
    /// <param name="allowExternalIds">
    ///     The parameter has as default true. If true only the external id property must match,
    ///     otherwise the "own" id property.
    /// </param>
    /// <param name="source">Specifies than the profile should match a special source.</param>
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
    /// <response code="404">If the request was not successful, because the specified container profile could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a <see cref="IProfile" />.
    ///     A <see cref="IProfile" /> can be a <see cref="Organization" />, <see cref="Group" /> or an <see cref="User" />.
    /// </returns>
    [HttpGet("{profileId}", Name = nameof(GetProfileByInternalOrExternalId))]
    [ProducesResponseType(typeof(IProfile), StatusCodes.Status200OK)]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetProfileByInternalOrExternalId(
        [FromRoute] [Required] string profileId,
        [FromQuery] bool allowExternalIds = true,
        [FromQuery] string source = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "ProfileId: {profileId}, allowExternalsIds: {allowExternalIds}, source: {source}",
                LogHelpers.Arguments(
                    profileId.ToLogString(),
                    allowExternalIds.ToLogString(),
                    source.ToLogString()));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            return BadRequest("The profileId is null, empty or contains only whitespaces.");
        }

        List<IProfile> foundProfile =
            await _readService.GetProfilesByExternalOrInternalIdAsync<User, Group, Organization>(
                profileId,
                allowExternalIds,
                source,
                cancellationToken);

        if (foundProfile.Count == 0)
        {
            string notFoundResponse = source == null
                ? $"The profile with the profileId: {profileId} could not be found."
                : $"The profile with the profileId: {profileId} and the source: {source} could not be found.";

            throw new InstanceNotFoundException(notFoundResponse);
        }

        if (foundProfile.Count > 1)
        {
            _logger.LogWarnMessage(
                "For the id: {profileId} where {foundProfile.Count} profiles found.",
                LogHelpers.Arguments(profileId.ToLogString(), foundProfile.Count.ToLogString()));

            HttpContext.Response.Headers.Add(
                // ReSharper disable once StringLiteralTypo
                "x-userprofileservice-warning",
                $"For the id: {profileId} where {foundProfile.Count} profiles found.");
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogWarnMessage(
                "Found {profile} for id: {profileId}: {ListOfProfiles}",
                LogHelpers.Arguments(
                    foundProfile.Count > 1 ? "profiles" : "profile",
                    profileId.ToLogString(),
                    foundProfile.ToLogString()));
        }

        IActionResult resultProfileActionResult = ActionResultHelper.ToActionResult(foundProfile.First());

        return _logger.ExitMethod(resultProfileActionResult);
    }
}
