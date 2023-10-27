using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Asp.Versioning;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Abstractions;
using UserProfileService.Api.Common.Attributes;
using UserProfileService.Api.Common.Configuration;
using UserProfileService.Api.Common.Extensions;
using UserProfileService.Attributes;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Configuration;
using UserProfileService.Extensions;
using UserProfileService.Models;
using UserProfileService.OpenApiSpec.Examples;
using UserProfileService.Utilities;

namespace UserProfileService.Controllers.V2;

/// <summary>
///     Manages the user settings stored in the UserProfileService.
///     The user settings are not stored in an ordinary way. They
///     are volatile and are stored in a separate data store and not
///     the event store like all other data.
/// </summary>
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[Route("api/v{version:apiVersion}/users")]
[SystemTextJsonSerializer]
public class UserSettingsController : Controller
{
    private readonly ILogger<UserSettingsController> _logger;
    private readonly IVolatileDataOperationHandler _operationHandler;
    private readonly IVolatileUserSettingsService _volatileUserSettingsService;

    /// <summary>
    ///     Creates an object of <see cref="UserSettingsController" />
    /// </summary>
    /// <param name="logger">The logger for logging purposes.</param>
    /// <param name="volatileUserSettingsService">
    ///     The volatile service is used to store and retrieve user settings/ user settings objects.
    /// </param>
    /// <param name="operationHandler">The operation handler that will take care of requests modifying volatile data state.</param>
    public UserSettingsController(
        ILogger<UserSettingsController> logger,
        IVolatileUserSettingsService volatileUserSettingsService,
        IVolatileDataOperationHandler operationHandler)
    {
        _logger = logger;
        _volatileUserSettingsService = volatileUserSettingsService;
        _operationHandler = operationHandler;
    }

    /// <summary>
    ///     Returns all user settings sections.
    /// </summary>
    /// <remarks>
    ///     Returns all user settings objects for a certain user and a certain user section name. <br />
    ///     The request can be filtered and sorted and its result will be a paginated list of objects.<br />
    ///     **Usage of the $filter query**
    ///     The $filter query is used to filter of result-set through restrictions. It can be seen as a where-Clause <br />
    ///     in the sense of SQL. The filter can only be applied to properties of the result item. Nested properties are not
    ///     supported yet.<br />
    ///     Filter expressions can be combined with an "OR" or an "AND". In the example below we have a simple data set that
    ///     <br />
    ///     represents a user that has only five properties. The filter can be applied of all of these properties.<br />
    ///     **Example Result-Item**
    ///     <code>
    ///         {   "Name": "Sam, Smith",
    ///             "FirstName":"Sam",
    ///             "LastName": "Smith",
    ///             "CreatedAt":"2022-09-11T09:30:11.6796611+02:00",
    ///             "Id": 235
    ///         }
    ///     </code>
    ///     **Example Filter-Expressions**
    ///     Let say we want all user that have the same last name. In a database there can be user with the same last name.
    ///     <br />
    ///     The query would look like:
    ///     <code> LastName eq 'Smith' </code>
    ///     The first item in the query is the property "LastName". As second item we have an operator (valid operators see
    ///     below).<br />
    ///     The third item is the value with which you want to compare. The value MUST be quoted if it is a type of string or
    ///     date (in out case all<br />
    ///     properties except "Id"). As quotation only single-quotation can be used. Valid quotes are ' and ′. Number must not
    ///     be quoted.<br />
    ///     **Valid operator are**
    ///     <code>
    ///         eq: EqualsOperator
    ///         ne: NotEqualsOperator
    ///         gt: GreaterThenOperator
    ///         ge: GreaterEqualsOperator
    ///         lt: LessThenOperator 
    ///         le: LessEqualsOperator 
    ///         ct: ContainsOperator
    ///   </code>
    ///     The operators can be applied nearly on every property. But for string only eq, ne and ct make sense. <br />
    ///     **Other Examples**
    ///     <code>LastName eq 'Smith' AND FirstName eq 'Sam'</code>
    ///     The user with the last name Smith and first name Sam should be filtered.<br /><br />
    ///     <code>Id eq 235</code>
    ///     The user with the Id 235 should be retrieved.<br /><br />
    ///     <code> CreatedAt lt '2023-09-11T09:30:11.6796611+02:00' AND Id ne 235 </code>
    ///     Return all user that were created below the data **'2023-09-11T09:30:11.6796611+02:00'** and **Sam, Smith** should
    ///     not be a part of <br />
    ///     the result set.
    ///     **PLEASE NOTICE**:
    ///     The explanation for the $filter was in general. Every endpoint has his own **RESULT**. So please look at the
    ///     Open-Api description which <br />
    ///     properties are available.<br />
    ///     **Usage of the $oderBy clause**
    ///     The $orderBy filter lets your order the result set by one or more properties. The properties must be again in the
    ///     result set.<br />
    ///     As our result set we will take again our simple example with the user. To order the user by the first name we can
    ///     using the query:
    ///     <code>FirstName</code>
    ///     This will order the user by its first name in an ascending order. Please notice that the default-value for the
    ///     sorting order is ascending.<br />
    ///     If you want to order the result set in a descending order you can use the query: <br /><br />
    ///     <code>FirstName desc</code>
    ///     If you forget the default sorting value you can still use the abbreviation asc in the query:<br /><br />
    ///     <code>FirstName asc</code>
    ///     It is also possible to sort by two properties. Again we using our simple user model to sort the results by first
    ///     name in a ascending order
    ///     and created at in descending order. Notice when using more than two properties you have to separate them with a
    ///     comma ",".
    ///     <code>FirstName, CreatedAt desc</code>
    ///     The filter query and the order by query can be combined.
    /// </remarks>
    /// <param name="userId">The used id that is used to retrieve all user setting section.</param>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A list of user settings sections that are stored for a certain user.</returns>
    [HttpGet("{userId}/UserSettingsSections", Name = nameof(GetAllUserSettingsSectionsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUserSettingsSectionsAsync(
        [Required] string userId,
        [FromQuery] QueryOptionsModel queryObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "userId: {userId}, QueryObject: {queryObject}",
                LogHelpers.Arguments(userId, queryObject.ToLogString()));
        }

        IPaginatedList<UserSettingSection> sectionsResult =
            await _volatileUserSettingsService.GetAllSettingsSectionsAsync(userId, queryObject, cancellationToken);

        return _logger.ExitMethod(
            ActionResultHelper.ToActionResult(sectionsResult.ToListResponseResult(HttpContext, queryObject)));
    }

    /// <summary>
    ///     Deletes a user settings section.
    /// </summary>
    /// <remarks>
    ///     Deletes a user settings section and all of its settings objects of a specific user.<br />
    ///     After accepting the request, the Location header contains the path to the status endpoint including a request id,
    ///     where the current status can be determined.
    /// </remarks>
    /// <param name="userId">The used id that is used to delete a user settings section.</param>
    /// <param name="userSettingsSectionName">The user settings section name that should be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A list of deleted <see cref="UserSettingObject" /> that were related to the
    ///     <paramref name="userSettingsSectionName" />
    ///     .
    /// </returns>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the <paramref name="userId" /> or
    ///     <paramref name="userSettingsSectionName" />
    ///     is not set.
    /// </response>
    /// <response code="404">
    ///     If the <paramref name="userId" /> or
    ///     <paramref name="userSettingsSectionName" />
    ///     could not be found.
    /// </response>
    /// <response code="409">When the <paramref name="userSettingsSectionName" /> does not exists.</response>
    /// <response code="500">
    ///     If a server-side error occurred and a <paramref name="userSettingsSectionName" /> could not be
    ///     deleted.
    /// </response>
    [HttpDelete("{userId}/UserSettingsSections/{userSettingsSectionName}", Name = nameof(DeleteSettingsKeyAsync))]
    [Consumes("application/json")]
    [Produces("application/json", "plain/text")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteSettingsKeyAsync(
        [Required] string userId,
        [Required] string userSettingsSectionName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteSettingsSectionForUserAsync(
                    userId,
                    userSettingsSectionName,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Creates a new user setting section.
    /// </summary>
    /// <remarks>
    ///     Creates a new section of user settings with values in the body. The body of the request must contain a string
    ///     representing a JSON array (using square brackets) that consists of JSON objects (in curly braces). <br />
    ///     After accepting the request, the Location header contains the path to the status endpoint including a request id,
    ///     where the current status can be determined. <br />
    ///     **Example body:**
    ///     <code>[{ "test": true, "testTwo": "yes"  }, { "amount": 5.2 }]</code>
    /// </remarks>
    /// <param name="userId">The user id for whose a user settings object should be stored. </param>
    /// <param name="userSettingsSectionName">
    ///     The user settings section name that is used to create a
    ///     <see cref="UserSettingObject" />.
    /// </param>
    /// <param name="userSettingObjects">
    ///     The user settings objects that includes user settings stored as an
    ///     array of JSONs objects.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The created user settings object that was stored for a certain user.</returns>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the <paramref name="userId" />, <paramref name="userSettingsSectionName" /> or
    ///     <paramref name="userSettingObjects" />
    ///     is not set.
    /// </response>
    /// <response code="404"> If the <paramref name="userId" />  could not be found.</response>
    /// <response code="500">
    ///     If a server-side error occurred and a <see cref="UserSettingObject" /> could not be
    ///     created for a certain user.
    /// </response>
    [HttpPost("{userId}/UserSettingsSections/{userSettingsSectionName}", Name = nameof(CreateUserSettingsAsync))]
    [Consumes("application/json")]
    [Produces("application/json", "plain/text")]
    [SetRequestBodyExample(typeof(CreateUserSettingsSectionExampleData))]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> CreateUserSettingsAsync(
        [Required] string userId,
        [Required] string userSettingsSectionName,
        [FromBody] [ModelBinder(typeof(JsonNodeModelBinder))] JsonArray userSettingObjects,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.CreateUserSettingAsync(
                    userId,
                    userSettingsSectionName,
                    userSettingObjects.ToJsonString(),
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Deletes a user settings object for a certain user in the database.
    /// </summary>
    /// <remarks>
    ///     Delete a specified user settings object that contains one entry of a user setting section. All other entries will
    ///     be kept. <br />
    ///     One entry is a previously provided JSON document (inside the curly braces). <br />
    ///     After accepting the request, the Location header contains the path to the status endpoint including a request id,
    ///     where the current status can be determined.
    /// </remarks>
    /// <param name="userId">The user id for whose a user settings object should be deleted. </param>
    /// <param name="userSettingsSectionName">The user settings section name that is needed to delete the user settings object.</param>
    /// <param name="userSettingsId">
    ///     The user settings id specify the exact user settings object that should be deleted for the
    ///     user and the settings section.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The deleted user settings object that was stored.</returns>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the <paramref name="userId" />, <paramref name="userSettingsSectionName" /> or
    ///     <paramref name="userSettingsId" />is not set.
    /// </response>
    /// <response code="500">
    ///     If a server-side error occurred and a <see cref="UserSettingObject" /> could not be
    ///     deleted for a certain user.
    /// </response>
    [HttpDelete(
        "{userId}/UserSettingsSections/{userSettingsSectionName}/children/{userSettingsId}",
        Name = nameof(DeleteUserSettingsAsync))]
    [Consumes("application/json")]
    [Produces("application/json", "plain/text")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> DeleteUserSettingsAsync(
        [FromRoute] [Required] string userId,
        [FromRoute] [Required] string userSettingsSectionName,
        [FromRoute] [Required] string userSettingsId,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.DeleteUserSettingsAsync(
                    userId,
                    userSettingsSectionName,
                    userSettingsId,
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Updates a user settings object for a certain user in the database.
    /// </summary>
    /// <remarks>
    ///     Updates a specified user settings object. The modified settings entry should be the body of the request.
    ///     It should be a JSON document - i.e. key/value pairs inside curly braces. <br />
    ///     After accepting the request, the Location header contains the path to the status endpoint including a request id,
    ///     where the current status can be determined. <br />
    ///     **Example body:**
    ///     <code>{ "existingProperty": "new value", "newProperty": 123.45  }</code>
    /// </remarks>
    /// <param name="userId">The user id which settings object should be updated. </param>
    /// <param name="userSettingsSectionName">
    ///     The user settings section name that  is used to update a
    ///     <see cref="UserSettingObject" />.
    /// </param>
    /// <param name="userSettingsId">
    ///     The exact id that identify the <see cref="userSettingsObject" /> where the value has to be
    ///     updated.
    /// </param>
    /// <param name="userSettingsObject">
    ///     The user settings object that should be updated for a certain user, a user
    ///     settings section and the exact user settings id.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The updated user settings object that was stored for a certain user.</returns>
    /// <response code="202">Request accepted. Redirects to operation status.</response>
    /// <response code="400">
    ///     If the <paramref name="userId" />, <paramref name="userSettingsSectionName" />,
    ///     <paramref name="userSettingsId" /> is not set.
    /// </response>
    /// <response code="404">
    ///     If the <paramref name="userId" />, <paramref name="userSettingsSectionName" /> or
    ///     <paramref name="userSettingsId" />  could not be found.
    /// </response>
    /// <response code="500">
    ///     If a server-side error occurred and a <paramref name="userSettingsSectionName" /> could not be
    ///     deleted for a certain user.
    /// </response>
    [HttpPut(
        "{userId}/UserSettingsSections/{userSettingsSectionName}/children/{userSettingsId}",
        Name = nameof(UpdateUserSettingsAsync))]
    [Consumes("application/json")]
    [Produces("application/json", "plain/text")]
    [AddHeaderParameters(WellKnownIdentitySettings.ImpersonateHeader)]
    public async Task<IActionResult> UpdateUserSettingsAsync(
        [FromRoute] [Required] string userId,
        [FromRoute] [Required] string userSettingsSectionName,
        [FromRoute] [Required] string userSettingsId,
        [FromBody] [ModelBinder(typeof(JsonNodeModelBinder))] JsonObject userSettingsObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IActionResult value = await ActionResultHelper.GetAcceptedAtStatusResultAsync(
            () =>
                _operationHandler.UpdateUserSettingsAsync(
                    userId,
                    userSettingsSectionName,
                    userSettingsId,
                    userSettingsObject.ToJsonString(),
                    cancellationToken),
            _logger);

        return _logger.ExitMethod(value);
    }

    /// <summary>
    ///     Returns user settings of a section.
    /// </summary>
    /// <remarks>
    ///     Returns all user settings objects for a certain user and a certain user section name. <br />
    ///     The request can be filtered and sorted and its result will be a paginated list of objects.<br />
    ///     **Usage of the $filter query**
    ///     The $filter query is used to filter of result-set through restrictions. It can be seen as a where-Clause <br />
    ///     in the sense of SQL. The filter can only be applied to properties of the result item. Nested properties are not
    ///     supported yet.<br />
    ///     Filter expressions can be combined with an "OR" or an "AND". In the example below we have a simple data set that
    ///     <br />
    ///     represents a user that has only five properties. The filter can be applied of all of these properties.<br />
    ///     **Example Result-Item**
    ///     <code>
    ///         {   "Name": "Sam, Smith",
    ///             "FirstName":"Sam",
    ///             "LastName": "Smith",
    ///             "CreatedAt":"2022-09-11T09:30:11.6796611+02:00",
    ///             "Id": 235
    ///         }
    ///     </code>
    ///     **Example Filter-Expressions**
    ///     Let say we want all user that have the same last name. In a database there can be user with the same last name.
    ///     <br />
    ///     The query would look like:
    ///     <code> LastName eq 'Smith' </code>
    ///     The first item in the query is the property "LastName". As second item we have an operator (valid operators see
    ///     below).<br />
    ///     The third item is the value with which you want to compare. The value MUST be quoted if it is a type of string or
    ///     date (in out case all<br />
    ///     properties except "Id"). As quotation only single-quotation can be used. Valid quotes are ' and ′. Number must not
    ///     be quoted.<br />
    ///     **Valid operator are**
    ///     <code>
    ///         eq: EqualsOperator
    ///         ne: NotEqualsOperator
    ///         gt: GreaterThenOperator
    ///         ge: GreaterEqualsOperator
    ///         lt: LessThenOperator 
    ///         le: LessEqualsOperator 
    ///         ct: ContainsOperator
    ///   </code>
    ///     The operators can be applied nearly on every property. But for string only eq, ne and ct make sense. <br />
    ///     **Other Examples**
    ///     <code>LastName eq 'Smith' AND FirstName eq 'Sam'</code>
    ///     The user with the last name Smith and first name Sam should be filtered.<br /><br />
    ///     <code>Id eq 235</code>
    ///     The user with the Id 235 should be retrieved.<br /><br />
    ///     <code> CreatedAt lt '2023-09-11T09:30:11.6796611+02:00' AND Id ne 235 </code>
    ///     Return all user that were created below the data **'2023-09-11T09:30:11.6796611+02:00'** and **Sam, Smith** should
    ///     not be a part of <br />
    ///     the result set.
    ///     **PLEASE NOTICE**:
    ///     The explanation for the $filter was in general. Every endpoint has his own **RESULT**. So please look at the
    ///     Open-Api description which <br />
    ///     properties are available.<br />
    ///     **Usage of the $oderBy clause**
    ///     The $orderBy filter lets your order the result set by one or more properties. The properties must be again in the
    ///     result set.<br />
    ///     As our result set we will take again our simple example with the user. To order the user by the first name we can
    ///     using the query:
    ///     <code>FirstName</code>
    ///     This will order the user by its first name in an ascending order. Please notice that the default-value for the
    ///     sorting order is ascending.<br />
    ///     If you want to order the result set in a descending order you can use the query: <br /><br />
    ///     <code>FirstName desc</code>
    ///     If you forget the default sorting value you can still use the abbreviation asc in the query:<br /><br />
    ///     <code>FirstName asc</code>
    ///     It is also possible to sort by two properties. Again we using our simple user model to sort the results by first
    ///     name in a ascending order
    ///     and created at in descending order. Notice when using more than two properties you have to separate them with a
    ///     comma ",".
    ///     <code>FirstName, CreatedAt desc</code>
    ///     The filter query and the order by query can be combined.
    /// </remarks>
    /// <param name="userId">The user id that is needed to find user settings objects.</param>
    /// <param name="userSettingsSectionName">The user settings section name to get the user settings objects.</param>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The <see cref="JsonObject" /> for the user and the certain user setting section.</returns>
    /// <response code="400">If the <paramref name="userId" /> or the <paramref name="userSettingsSectionName" /> is not set.</response>
    /// <response code="404">
    ///     If the <paramref name="userId" /> or the <paramref name="userSettingsSectionName" /> could not be
    ///     found.
    /// </response>
    /// <response code="500">
    ///     If a server-side error occurred and a user settings object could not be
    ///     retrieved for a certain user.
    /// </response>
    [HttpGet(
        "{userId}/UserSettingsSections/{userSettingsSectionName}/children",
        Name = nameof(GetUserSettingObjectsForSectionAsync))]
    [ProducesResponseType(typeof(ListResponseResult<UserSettingObject>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserSettingObjectsForSectionAsync(
        [FromRoute] string userId,
        [FromRoute] string userSettingsSectionName,
        [FromQuery] QueryOptionsModel queryObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "userId: {userId}, userSettingsSectionName: {userSettingsSectionName}, paginationObject:{queryObject}",
                LogHelpers.Arguments(userId, userSettingsSectionName, queryObject.ToLogString()));
        }

        IPaginatedList<UserSettingObject> result = await _volatileUserSettingsService.GetUserSettingsAsync(
            userId,
            userSettingsSectionName,
            queryObject,
            cancellationToken);

        return _logger.ExitMethod(
            ActionResultHelper.ToActionResult(result.ToListResponseResult(HttpContext, queryObject)));
    }

    /// <summary>
    ///     Returns all existing user settings.
    /// </summary>
    /// <remarks>
    ///     Returns all user settings objects for a certain user and a certain user section name. <br />
    ///     The request can be filtered and sorted and its result will be a paginated list of objects.<br />
    ///     **Usage of the $filter query**
    ///     The $filter query is used to filter of result-set through restrictions. It can be seen as a where-Clause <br />
    ///     in the sense of SQL. The filter can only be applied to properties of the result item. Nested properties are not
    ///     supported yet.<br />
    ///     Filter expressions can be combined with an "OR" or an "AND". In the example below we have a simple data set that
    ///     <br />
    ///     represents a user that has only five properties. The filter can be applied of all of these properties.<br />
    ///     **Example Result-Item**
    ///     <code>
    ///         {   "Name": "Sam, Smith",
    ///             "FirstName":"Sam",
    ///             "LastName": "Smith",
    ///             "CreatedAt":"2022-09-11T09:30:11.6796611+02:00",
    ///             "Id": 235
    ///         }
    ///     </code>
    ///     **Example Filter-Expressions**
    ///     Let say we want all user that have the same last name. In a database there can be user with the same last name.
    ///     <br />
    ///     The query would look like:
    ///     <code> LastName eq 'Smith' </code>
    ///     The first item in the query is the property "LastName". As second item we have an operator (valid operators see
    ///     below).<br />
    ///     The third item is the value with which you want to compare. The value MUST be quoted if it is a type of string or
    ///     date (in out case all<br />
    ///     properties except "Id"). As quotation only single-quotation can be used. Valid quotes are ' and ′. Number must not
    ///     be quoted.<br />
    ///     **Valid operator are**
    ///     <code>
    ///         eq: EqualsOperator
    ///         ne: NotEqualsOperator
    ///         gt: GreaterThenOperator
    ///         ge: GreaterEqualsOperator
    ///         lt: LessThenOperator 
    ///         le: LessEqualsOperator 
    ///         ct: ContainsOperator
    ///   </code>
    ///     The operators can be applied nearly on every property. But for string only eq, ne and ct make sense. <br />
    ///     **Other Examples**
    ///     <code>LastName eq 'Smith' AND FirstName eq 'Sam'</code>
    ///     The user with the last name Smith and first name Sam should be filtered.<br /><br />
    ///     <code>Id eq 235</code>
    ///     The user with the Id 235 should be retrieved.<br /><br />
    ///     <code> CreatedAt lt '2023-09-11T09:30:11.6796611+02:00' AND Id ne 235 </code>
    ///     Return all user that were created below the data **'2023-09-11T09:30:11.6796611+02:00'** and **Sam, Smith** should
    ///     not be a part of <br />
    ///     the result set.
    ///     **PLEASE NOTICE**:
    ///     The explanation for the $filter was in general. Every endpoint has his own **RESULT**. So please look at the
    ///     Open-Api description which <br />
    ///     properties are available.<br />
    ///     **Usage of the $oderBy clause**
    ///     The $orderBy filter lets your order the result set by one or more properties. The properties must be again in the
    ///     result set.<br />
    ///     As our result set we will take again our simple example with the user. To order the user by the first name we can
    ///     using the query:
    ///     <code>FirstName</code>
    ///     This will order the user by its first name in an ascending order. Please notice that the default-value for the
    ///     sorting order is ascending.<br />
    ///     If you want to order the result set in a descending order you can use the query: <br /><br />
    ///     <code>FirstName desc</code>
    ///     If you forget the default sorting value you can still use the abbreviation asc in the query:<br /><br />
    ///     <code>FirstName asc</code>
    ///     It is also possible to sort by two properties. Again we using our simple user model to sort the results by first
    ///     name in a ascending order
    ///     and created at in descending order. Notice when using more than two properties you have to separate them with a
    ///     comma ",".
    ///     <code>FirstName, CreatedAt desc</code>
    ///     The filter query and the order by query can be combined.
    /// </remarks>
    /// <param name="userId">The user id that is needed to find all existing user settings objects.</param>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>The <see cref="UserSettingObject" /> for the appropriate user.</returns>
    /// <response code="400">If the <paramref name="userId" /> is not set.</response>
    /// <response code="404">
    ///     If the <paramref name="userId" /> could not be found.
    /// </response>
    /// <response code="500">
    ///     If a server-side error occurred and a user settings object could not be
    ///     retrieved for a certain user.
    /// </response>
    [HttpGet("{userId}/UserSettingsSections/allChildren", Name = nameof(GetAllUserSettingsAsync))]
    [ProducesResponseType(typeof(ListResponseResult<UserSettingObject>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUserSettingsAsync(
        [FromRoute] string userId,
        [FromQuery] QueryOptionsModel queryObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "userId: {userId}, queryObject: {queryObject}",
                LogHelpers.Arguments(userId, queryObject.ToString()));
        }

        IPaginatedList<UserSettingObject> result =
            await _volatileUserSettingsService.GetAllUserSettingObjectForUserAsync(
                userId,
                queryObject,
                cancellationToken);

        return _logger.ExitMethod(
            ActionResultHelper.ToActionResult(result.ToListResponseResult(HttpContext, queryObject)));
    }
}
