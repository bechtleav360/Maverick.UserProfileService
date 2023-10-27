using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using AutoMapper;
using Maverick.UserProfileService.Models.Abstraction;
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
using UserProfileService.Extensions;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     All methods related to tags.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}")]
public class TagsController : ControllerBase
{
    private readonly ILogger<TagsController> _logger;
    private readonly IMapper _mapper;
    private readonly IOperationHandler _operationHandler;
    private readonly IReadService _readService;
    private readonly IUserContextStore _userContextStore;

    public TagsController(
        ILoggerFactory loggerFactory,
        IOperationHandler operationHandler,
        IReadService readService,
        IUserContextStore userContextStore,
        IMapper mapper)
    {
        _logger = loggerFactory.CreateLogger<TagsController>();
        _operationHandler = operationHandler;
        _readService = readService;
        _userContextStore = userContextStore;
        _mapper = mapper;
    }

    /// <summary>
    ///     Returns a list of tags.
    /// </summary>
    /// <param name="queryObject">The options object that will set up list query (for pagination, sort order, filter, etc.).</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and a list of tags has been returned.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>A list of tags</returns>
    [HttpGet("tags", Name = nameof(GetTagsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<Tag>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTagsAsync(
        [FromQuery] QueryObjectTags queryObject = null,
        CancellationToken cancellationToken = default)

    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "QueryObjectTags: {queryObject}.",
                LogHelpers.Arguments(queryObject.ToLogString()));
        }

        IPaginatedList<Tag> tags =
            await _readService
                .GetTagsAsync(
                    _mapper.Map<QueryObject>(queryObject),
                    cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(
                tags.ToListResponseResult(HttpContext, queryObject) ?? new ListResponseResult<Tag>());

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns the specific tag.
    /// </summary>
    /// <param name="id">The id of the tag to get.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and the tag is in the body of the response.</response>
    /// <response code="400">If the request was not valid.An error object has been returned that contains detailed information.</response>
    /// <response code="401">Required authentication information is either missing or not valid for the resource.</response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="404">If the request was not successful, because the specified tag could not be found.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Return the specified tag.</returns>
    [HttpGet("tags/{id}", Name = nameof(GetTagAsync))]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTagAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "TagId: {Id}",
                LogHelpers.Arguments(id));
        }

        Tag tag =
            await _readService
                .GetTagAsync(
                    id,
                    cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(tag);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Creates a new tag.
    /// </summary>
    /// <param name="tagProperties">The tag properties that can be set when creating a tag.</param>
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
    /// <response code="409">If the tag already exists.</response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>Returns the created tag.</returns>
    [HttpPost("tags", Name = nameof(CreateTagAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateTagAsync(
        [FromBody] CreateTagRequest tagProperties,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "CreateTagRequest: {createTagRequest}",
                LogHelpers.Arguments(tagProperties.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateTagAsync(tagProperties, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Deletes a specific tag.
    /// </summary>
    /// <param name="id">The id of the tag has to be deleted.</param>
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
    /// <returns>The deleted tag.</returns>
    [HttpDelete("tags/{id}", Name = nameof(DeleteTagAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteTagAsync(
        [FromRoute] [Required] string id,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "TagId: {id}",
                LogHelpers.Arguments(id));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteTagAsync(id, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets existing tag objects of current user profile. If no tags could be found, an empty list will be returned.
    /// </summary>
    /// <param name="tagType"> The type of the tag. </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200"> If the request was successful and a collection of requested assigned tags has been returned. </response>
    /// <response code="401"> Required authentication information is either missing or not valid for the resource. </response>
    /// <response code="403">
    ///     Access is denied to the requested resource.The user might not have enough permission.The response
    ///     body contains an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns> The requested list of tags, if some were found. Otherwise an empty list. </returns>
    [Produces("application/json", "plain/text")]
    [ProducesResponseType(typeof(ListResponseResult<CalculatedTag>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpGet("users/me/tags", Name = nameof(GetTagsOfOwnProfileAsync))]
    public async Task<IActionResult> GetTagsOfOwnProfileAsync(
        RequestedTagType tagType = RequestedTagType.All,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();
        string currentUserId = await _userContextStore.GetIdOfCurrentUserAsync();

        IPaginatedList<CalculatedTag> tags =
            await _readService
                .GetTagsOfProfileAsync(
                    currentUserId,
                    tagType,
                    cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(
                tags.ToListResponseResult(HttpContext) ?? new ListResponseResult<CalculatedTag>());

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets existing tag objects of the specified user profile. If no tags could be found, an empty list will be returned.
    /// </summary>
    /// <param name="id">The id of the user whose tags should be returned..</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <param name="tagType">The type of the tag. For for information see <see cref="RequestedTagType" />.</param>
    /// <response code="200">If the request was successful and a collection of requested assigned tags has been returned.</response>
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
    /// <returns>The requested list of tags, if some were found. Otherwise an empty list.</returns>
    [Produces("application/json", "plain/text")]
    [ProducesResponseType(typeof(ListResponseResult<CalculatedTag>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpGet("users/{id}/tags", Name = nameof(GetTagsOfUserProfileAsync))]
    public async Task<IActionResult> GetTagsOfUserProfileAsync(
        string id,
        RequestedTagType tagType = RequestedTagType.All,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserProfileId: {id}",
                LogHelpers.Arguments(id));
        }

        IPaginatedList<CalculatedTag> tags =
            await _readService.GetTagsOfProfileAsync(
                id,
                tagType,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(
                tags.ToListResponseResult(HttpContext) ?? new ListResponseResult<CalculatedTag>());

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Adds new tag objects to specified user profile.
    /// </summary>
    /// <param name="id">The id of the user to be modified.</param>
    /// <param name="tags">The tags to create or update.</param>
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
    /// <returns>The updated or created tags.</returns>
    [HttpPost("users/{id}/tags", Name = nameof(CreateOrUpdateToUserProfileAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateOrUpdateToUserProfileAsync(
        [FromRoute] [Required] string id,
        [FromBody] TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {usrId}, tagsAssignment: {tagsAssignments} ",
                LogHelpers.Arguments(id, tags.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateProfileTagsAsync(
                    id,
                    ProfileKind.User,
                    tags,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes specified tag names from specified user profile.
    /// </summary>
    /// <param name="id">The id of the user to be modified.</param>
    /// <param name="tagId">The id of the tag to be removed from a specified user.</param>
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
    ///     If either the specified user or the specified tag could not be found. The response body contains
    ///     an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpDelete("users/{id}/tags/{tagId}", Name = nameof(RemoveTagFromUserProfileAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> RemoveTagFromUserProfileAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string tagId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "UserId: {userId}, TagId: {tagId}",
                LogHelpers.Arguments(id, tagId));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteTagsFromProfile(
                    id,
                    ProfileKind.User,
                    new[] { tagId },
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Gets existing tag objects of the specified group profile. If no tags could be found, an empty list will be
    ///     returned.
    /// </summary>
    /// <param name="id">The id of the group whose tags should be returned..</param>
    /// <param name="tagType">The type of the tag.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <response code="200">If the request was successful and a collection of requested assigned tags has been returned.</response>
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
    /// <returns>The requested list of tags, if some were found. Otherwise an empty list.</returns>
    [Produces("application/json", "plain/text")]
    [ProducesResponseType(typeof(ListResponseResult<CalculatedTag>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [HttpGet("groups/{id}/tags", Name = nameof(GetTagsOfGroupProfileAsync))]
    public async Task<IActionResult> GetTagsOfGroupProfileAsync(
        string id,
        RequestedTagType tagType = RequestedTagType.All,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}.",
                LogHelpers.Arguments(id));
        }

        IPaginatedList<CalculatedTag> tags =
            await _readService.GetTagsOfProfileAsync(
                id,
                tagType,
                cancellationToken);

        IActionResult value =
            ActionResultHelper.ToActionResult(
                tags.ToListResponseResult(HttpContext) ?? new ListResponseResult<CalculatedTag>());

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Adds new tag objects to specified group profile.
    /// </summary>
    /// <param name="id">The id of the group to be modified.</param>
    /// <param name="tags">The tags to create or update.</param>
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
    ///     If either the specified group or the specified tag could not be found.The response body contains
    ///     an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpPost("groups/{id}/tags", Name = nameof(CreateOrUpdateTagsOfGroupProfileAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateOrUpdateTagsOfGroupProfileAsync(
        [FromRoute] [Required] string id,
        [FromBody] TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, tagsAssignments: {tagsAssignments}.",
                LogHelpers.Arguments(id));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateProfileTagsAsync(
                    id,
                    ProfileKind.Group,
                    tags,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes specified tag names from the specified group profile.
    /// </summary>
    /// <param name="id">The id of the group to be modified.</param>
    /// <param name="tagId">The id of the tag to be removed from specified group.</param>
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
    ///     If either the specified group or the specified tag could not be found. The response body contains
    ///     an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpDelete("groups/{id}/tags/{tagId}", Name = nameof(RemoveTagFromGroupProfileAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> RemoveTagFromGroupProfileAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string tagId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "GroupId: {groupId}, TagId: {tagId}.",
                LogHelpers.Arguments(id, tagId));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteTagsFromProfile(
                    id,
                    ProfileKind.Group,
                    new[] { tagId },
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Adds new tag objects to specified role.
    /// </summary>
    /// <param name="id">The id of the role to be modified.</param>
    /// <param name="tags">The tags to create or update.</param>
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
    ///     If either the specified group or the specified tag could not be found.The response body contains
    ///     an error object with detailed information..
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpPost("roles/{id}/tags", Name = nameof(CreateOrUpdateTagsFromRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateOrUpdateTagsFromRoleAsync(
        [FromRoute] [Required] string id,
        [FromBody] TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}, tagsAssignments {tagsAssignments}",
                LogHelpers.Arguments(id, tags.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateRoleTagsAsync(id, tags, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes the specified tag name from the specified role.
    /// </summary>
    /// <param name="id">The id of the role to be modified.</param>
    /// <param name="tagId">The id of the tag to be removed from specified object.</param>
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
    ///     If either the specified group or the specified tag could not be found. The response body contains
    ///     an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpDelete("roles/{id}/tags/{tagId}", Name = nameof(DeleteTagOfRoleAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteTagOfRoleAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string tagId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "RoleId: {roleId}, TagId {tagId}",
                LogHelpers.Arguments(id, tagId));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteTagsFromRole(
                    id,
                    new[] { tagId },
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Adds new tag objects to specified function.
    /// </summary>
    /// <param name="id">The id of the function to be modified.</param>
    /// <param name="tags">The tags to create or update.</param>
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
    ///     If either the specified group or the specified tag could not be found.The response body contains
    ///     an error object with detailed information..
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpPost("functions/{id}/tags", Name = nameof(CreateOrUpdateTagsFromFunctionAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateOrUpdateTagsFromFunctionAsync(
        [FromRoute] [Required] string id,
        [FromBody] TagAssignment[] tags,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "FunctionId: {funcId}, TagAssignment {tagAssignment}",
                LogHelpers.Arguments(id, tags.ToLogString()));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateFunctionTagsAsync(id, tags, cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Removes the specified tag name from the specified function.
    /// </summary>
    /// <param name="id">The id of the function to be modified.</param>
    /// <param name="tagId">The id of the tag to be removed from specified object.</param>
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
    ///     If either the specified group or the specified tag could not be found. The response body contains
    ///     an error object with detailed information.
    /// </response>
    /// <response code="500">
    ///     There was an internal server error while processing the request. The response body contains an
    ///     error object with detailed information.
    /// </response>
    /// <returns>The updated or created tags.</returns>
    [HttpDelete("functions/{id}/tags/{tagId}", Name = nameof(DeleteTagOfFunctionAsync))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteTagOfFunctionAsync(
        [FromRoute] [Required] string id,
        [FromRoute] [Required] string tagId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "FunctionId: {funcId}, TagId {tagId}",
                LogHelpers.Arguments(id, tagId));
        }

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteTagsFromFunction(
                    id,
                    new[] { tagId },
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }
}
