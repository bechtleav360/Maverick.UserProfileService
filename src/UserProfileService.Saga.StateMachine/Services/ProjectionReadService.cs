using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Utilities;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Implementation of <see cref="IProjectionReadService" />
/// </summary>
public class ProjectionReadService : IProjectionReadService, IValidationReadService
{
    private readonly ILogger<ProjectionReadService> _logger;
    private readonly IReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProjectionReadService" />
    /// </summary>
    /// <param name="readService">Read service where the entities are stored.</param>
    /// <param name="logger">The logger that is used to log messages with various severities.</param>
    public ProjectionReadService(IReadService readService, ILogger<ProjectionReadService> logger)
    {
        _readService = readService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> CheckProfileExistsAsync(string id, ProfileKind profileKind)
    {
        _logger.EnterMethod();

        try
        {
            await _readService.GetProfileAsync<IProfile>(id, profileKind.ConvertToRequestedProfileKind());
            _logger.LogDebugMessage("Found function with id '{id}'.", LogHelpers.Arguments(id));

            return _logger.ExitMethod(true);
        }
        catch (InstanceNotFoundException)
        {
            return _logger.ExitMethod(false);
        }
    }

    /// <summary>
    ///     The method checks if the the given object exists already in the database.
    /// </summary>
    /// <param name="objectIdent">
    ///     The <see cref="IObjectIdent" /> that contains the id and the type of the objects that should
    ///     be checked for existence.
    /// </param>
    /// <returns>True if the objects already exists, otherwise false.</returns>
    public async Task<bool> CheckObjectExistsAsync(IObjectIdent objectIdent)
    {
        _logger.EnterMethod();

        try
        {
            bool result;

            switch (objectIdent.Type)
            {
                case ObjectType.Function:
                    var function = await _readService.GetFunctionAsync<FunctionView>(objectIdent.Id);
                    result = function != null;

                    break;
                case ObjectType.Role:
                    RoleView role = await _readService.GetRoleAsync(objectIdent.Id);
                    result = role != null;

                    break;
                case ObjectType.Tag:
                    Tag tag = await _readService.GetTagAsync(objectIdent.Id);
                    result = tag != null;

                    break;
                case ObjectType.Group:
                case ObjectType.Organization:
                case ObjectType.User:
                case ObjectType.Profile:
                case ObjectType.Unknown
                    : // Unknown is only used for profile assignments and validated in saga worker.
                    var profile =
                        await _readService.GetProfileAsync<IProfile>(objectIdent.Id, RequestedProfileKind.All);

                    result = profile != null;

                    if (result)
                    {
                        objectIdent.Type = profile.Kind.ToObjectType();
                    }

                    break;
                default:
                    _logger.LogErrorMessage(
                        null,
                        "The following object type '{type}' is not implemented to retrieve object infos.",
                        LogHelpers.Arguments(objectIdent.Type));

                    result = false;

                    break;
            }

            return _logger.ExitMethod(result);
        }
        catch (InstanceNotFoundException)
        {
            _logger.LogTraceMessage(
                "Object with id '{id}' and type '{type}' could not be found.",
                LogHelpers.Arguments(
                    objectIdent.Id,
                    objectIdent.Type));

            return _logger.ExitMethod(false);
        }
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, bool>> CheckTagsExistAsync(params string[] ids)
    {
        _logger.EnterMethod();

        if (ids == null || !ids.Any())
        {
            return new Dictionary<string, bool>();
        }

        IEnumerable<string> tags = await _readService.GetExistentTagsAsync(ids);

        IDictionary<string, bool> dictExists = ids.ToDictionary(t => t, t => tags.Contains(t));

        return _logger.ExitMethod(dictExists);
    }

    /// <inheritdoc />
    public async Task<bool> CheckUserEmailExistsAsync(string email, string userId = "")
    {
        _logger.EnterMethod();

        var query = new AssignmentQueryObject
        {
            Filter = new Filter
            {
                CombinedBy = BinaryOperator.And,
                Definition = new List<Maverick.UserProfileService.Models.RequestModels.Definitions>
                {
                    new Maverick.UserProfileService.Models.RequestModels.Definitions
                    {
                        FieldName = nameof(UserBasic.Email),
                        Values = new[] { email },
                        BinaryOperator = BinaryOperator.And,
                        Operator = FilterOperator.Equals
                    }
                }
            }
        };

        IPaginatedList<IProfile> result =
            await _readService.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                RequestedProfileKind.User,
                query);

        if (result.TotalAmount != 0)
        {
            bool duplicate = result.All(r => r.Id != userId);

            _logger.LogDebugMessage("Found user with email '{email}'", LogHelpers.Arguments(email));

            return _logger.ExitMethod(duplicate);
        }

        _logger.LogDebugMessage("No user found with email '{email}'´.", LogHelpers.Arguments(email));

        return _logger.ExitMethod(false);
    }

    /// <inheritdoc />
    public async Task<bool> CheckGroupNameExistsAsync(
        string name,
        string displayName,
        bool ignoreCase,
        string groupId = "")
    {
        _logger.EnterMethod();

        var query = new AssignmentQueryObject
        {
            Filter = new Filter
            {
                CombinedBy = BinaryOperator.Or,
                Definition = new List<Maverick.UserProfileService.Models.RequestModels.Definitions>
                {
                    new Maverick.UserProfileService.Models.RequestModels.Definitions
                    {
                        FieldName = nameof(GroupBasic.Name),
                        Values = new[] { name, displayName },
                        BinaryOperator = BinaryOperator.Or,
                        Operator = ignoreCase
                            ? FilterOperator.Contains
                            : FilterOperator.Equals
                    },
                    new Maverick.UserProfileService.Models.RequestModels.Definitions
                    {
                        FieldName = nameof(GroupBasic.DisplayName),
                        Values = new[] { name, displayName },
                        BinaryOperator = BinaryOperator.Or,
                        Operator = ignoreCase
                            ? FilterOperator.Contains
                            : FilterOperator.Equals
                    }
                }
            }
        };

        IPaginatedList<IProfile> result =
            await _readService.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                RequestedProfileKind.Group,
                query);

        if (result.TotalAmount != 0)
        {
            var sameNameList = new List<IProfile>();

            if (ignoreCase)
            {
                sameNameList = result.Where(
                        r => ValidationLogicProvider.Group.CompareNames(
                            r.Name,
                            r.DisplayName,
                            name,
                            displayName))
                    .ToList();
            }

            bool duplicate = sameNameList.Any() && sameNameList.All(s => s.Id != groupId);

            _logger.LogDebugMessage("Found group with name '{name}'", LogHelpers.Arguments(name));

            return _logger.ExitMethod(duplicate);
        }

        _logger.LogDebugMessage("No group found with name '{name}'.", LogHelpers.Arguments(name));

        return _logger.ExitMethod(false);
    }

    /// <inheritdoc cref="IProjectionReadService.GetProfileAsync" />
    public async Task<IProfile> GetProfileAsync(string id, ProfileKind profileKind)
    {
        _logger.EnterMethod();

        try
        {
            var profile = await _readService.GetProfileAsync<IProfile>(
                id,
                profileKind.ConvertToRequestedProfileKind());

            _logger.LogDebugMessage(
                "Found profile with id '{id}' and kind {profile.Kind}.",
                LogHelpers.Arguments(id, profile.Kind));

            return profile;
        }
        catch (InstanceNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc cref="IProjectionReadService.GetProfilesAsync"/>
    public async Task<ICollection<IProfile>> GetProfilesAsync(ICollection<string> ids, ProfileKind profileKind)
    {
        _logger.EnterMethod();

        IPaginatedList<IProfile> profiles =
            await _readService.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                ids,
                profileKind.ConvertToRequestedProfileKind());

        return _logger.ExitMethod(profiles);
    }

    /// <inheritdoc cref= "IProjectionReadService.GetProfilesAsync"/>
    public async Task<Tag> GetTagAsync(string id)
    {
        _logger.EnterMethod();

        try
        {
            Tag tag = await _readService.GetTagAsync(id);

            return _logger.ExitMethod(tag);
        }
        catch (InstanceNotFoundException)
        {
            return _logger.ExitMethod<Tag>(null);
        }
    }

    /// <summary>
    ///     Returns a JSON object that contains all settings of a profile with the specified config key.
    /// </summary>
    /// <param name="profileId">The id of the profile whose settings should be returned.</param>
    /// <param name="profileKind"></param>
    /// <param name="settingsKey">The key of the config that contains the requested settings.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a <see cref="JObject" /> that represents the
    ///     requested settings.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="profileId" /> is empty or contains only whitespace characters.<br />-or-<br />
    ///     <paramref name="settingsKey" /> is empty or contains only whitespace characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="profileId" /> is <c>null</c>.<br />-or-<br />
    ///     <paramref name="settingsKey" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    public async Task<JObject> GetSettingsOfProfileAsync(
        string profileId,
        ProfileKind profileKind,
        string settingsKey)
    {
        _logger.EnterMethod();

        JObject settings = await _readService.GetSettingsOfProfileAsync(profileId, profileKind, settingsKey);

        return _logger.ExitMethod(settings);
    }

    /// <inheritdoc cref="IProjectionReadService.GetFunctionAsync"/>
    public async Task<FunctionView> GetFunctionAsync(string id)
    {
        _logger.EnterMethod();

        try
        {
            var function = await _readService.GetFunctionAsync<FunctionView>(id);

            return _logger.ExitMethod(function);
        }
        catch (InstanceNotFoundException)
        {
            return _logger.ExitMethod<FunctionView>(null);
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<FunctionBasic>> GetFunctionsAsync(string roleId, string organizationId)
    {
        _logger.EnterMethod();

        try
        {
            var query = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    CombinedBy = BinaryOperator.And,
                    Definition = new List<Maverick.UserProfileService.Models.RequestModels.Definitions>
                    {
                        new Maverick.UserProfileService.Models.RequestModels.Definitions
                        {
                            FieldName =
                                $"{nameof(FunctionBasic.Role)}.{nameof(RoleBasic.Id)}",
                            Values = new[] { roleId },
                            BinaryOperator = BinaryOperator.And,
                            Operator = FilterOperator.Equals
                        },
                        new Maverick.UserProfileService.Models.RequestModels.Definitions
                        {
                            FieldName =
                                $"{nameof(FunctionBasic.Organization)}.{nameof(OrganizationBasic.Id)}",
                            Values = new[] { organizationId },
                            BinaryOperator = BinaryOperator.And,
                            Operator = FilterOperator.Equals
                        }
                    }
                }
            };

            IPaginatedList<FunctionBasic> functions = await _readService.GetFunctionsAsync<FunctionBasic>(query);

            return _logger.ExitMethod(functions);
        }
        catch (InstanceNotFoundException)
        {
            return _logger.ExitMethod<ICollection<FunctionBasic>>(new List<FunctionBasic>());
        }
    }

    /// <inheritdoc cref="IProjectionReadService.GetRoleAsync"/>
    public async Task<RoleBasic> GetRoleAsync(string id)
    {
        _logger.EnterMethod();

        try
        {
            RoleView role = await _readService.GetRoleAsync(id);

            return _logger.ExitMethod(role);
        }
        catch (InstanceNotFoundException)
        {
            return _logger.ExitMethod<RoleBasic>(null);
        }
    }

    /// <inheritdoc />
    public async Task<string[]> GetRoleFunctionAssignmentsAsync(string roleId)
    {
        _logger.EnterMethod();

        var query = new AssignmentQueryObject
        {
            Filter = new Filter
            {
                CombinedBy = BinaryOperator.And,
                Definition = new List<Maverick.UserProfileService.Models.RequestModels.Definitions>
                {
                    new Maverick.UserProfileService.Models.RequestModels.Definitions
                    {
                        FieldName =
                            $"{nameof(FunctionBasic.Role)}.{nameof(RoleBasic.Id)}",
                        Values = new[] { roleId },
                        BinaryOperator = BinaryOperator.And,
                        Operator = FilterOperator.Equals
                    }
                }
            },
            Limit = 10
        };

        IPaginatedList<FunctionView> paginatedList = await _readService.GetFunctionsAsync<FunctionView>(query);

        string[] functionIds = paginatedList
            .Select(p => p.Id)
            .ToArray();

        return _logger.ExitMethod(functionIds);
    }

    /// <inheritdoc />
    public Task<string[]> GetAllParentsOfProfile(string id)
    {
        // Todo: Valid implementation missing - due of read service changes this method was not working any more
        _logger.EnterMethod();

        return _logger.ExitMethod(Task.FromResult(Array.Empty<string>()));
    }

    /// <inheritdoc />
    public async Task<ICollection<ProfileIdent>> GetParentsOfProfileAsync(string id)
    {
        _logger.EnterMethod();

        try
        {
            IPaginatedList<IContainerProfile> parents = await _readService
                .GetParentsOfProfileAsync<GroupBasic, OrganizationBasic>(id, RequestedProfileKind.All);

            ProfileIdent[] parentIds = parents.Select(p => new ProfileIdent(p.Id, p.Kind)).ToArray();

            if (!parentIds.Any())
            {
                _logger.LogDebugMessage("No parents found for profile with id '{id}'", LogHelpers.Arguments(id));
            }

            return _logger.ExitMethod(parentIds);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(e, e.Message, LogHelpers.Arguments(id));

            return _logger.ExitMethod(new List<ProfileIdent>());
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<ProfileIdent>> GetChildrenOfProfileAsync(string id, ProfileKind profileKind)
    {
        _logger.EnterMethod();

        try
        {
            IPaginatedList<IProfile> children = await _readService
                .GetChildrenOfProfileAsync<UserBasic, GroupBasic, OrganizationBasic>(
                    id,
                    profileKind.ToContainerProfileType());

            ProfileIdent[] parentIds = children.Select(p => new ProfileIdent(p.Id, p.Kind)).ToArray();

            if (!parentIds.Any())
            {
                _logger.LogDebugMessage("No children found for profile with id '{id}'", LogHelpers.Arguments(id));
            }

            return _logger.ExitMethod(parentIds);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(e, e.Message, LogHelpers.Arguments(id));

            return _logger.ExitMethod(new List<ProfileIdent>());
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<Member>> GetAssignedProfilesAsync(string roleOrFunctionId)
    {
        _logger.EnterMethod();

        IPaginatedList<Member> assignedMembers = await _readService.GetAssignedProfiles(roleOrFunctionId);

        return _logger.ExitMethod(assignedMembers);
    }
}
