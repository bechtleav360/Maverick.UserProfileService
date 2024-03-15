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
    /// <summary>
    ///     Gets the logger instance for this instance.
    /// </summary>
    /// <remarks>
    ///     The logger is used to record and handle log messages related to the projection read service.
    ///     Use this logger to log events, errors, and other relevant information during the execution of the service.
    /// </remarks>
    protected ILogger Logger { get; }

    private readonly IReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProjectionReadService" />
    /// </summary>
    /// <param name="readService">Read service where the entities are stored.</param>
    /// <param name="logger">The logger that is used to log messages with various severities.</param>
    public ProjectionReadService(IReadService readService, ILogger<ProjectionReadService> logger)
    {
        _readService = readService;
        Logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> CheckProfileExistsAsync(string id, ProfileKind profileKind)
    {
        Logger.EnterMethod();

        try
        {
            await _readService.GetProfileAsync<IProfile>(id, profileKind.ConvertToRequestedProfileKind());
            Logger.LogDebugMessage("Found function with id '{id}'.", LogHelpers.Arguments(id));

            return Logger.ExitMethod(true);
        }
        catch (InstanceNotFoundException)
        {
            return Logger.ExitMethod(false);
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
        Logger.EnterMethod();

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

                    if (profile != null)
                    {
                        objectIdent.Type = profile.Kind.ToObjectType();
                    }

                    break;
                default:
                    Logger.LogErrorMessage(
                        null,
                        "The following object type '{type}' is not implemented to retrieve object infos.",
                        LogHelpers.Arguments(objectIdent.Type));

                    result = false;

                    break;
            }

            return Logger.ExitMethod(result);
        }
        catch (InstanceNotFoundException)
        {
            Logger.LogTraceMessage(
                "Object with id '{id}' and type '{type}' could not be found.",
                LogHelpers.Arguments(
                    objectIdent.Id,
                    objectIdent.Type));

            return Logger.ExitMethod(false);
        }
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, bool>> CheckTagsExistAsync(params string[] ids)
    {
        Logger.EnterMethod();

        if (ids == null || !ids.Any())
        {
            return new Dictionary<string, bool>();
        }

        IEnumerable<string> tags = await _readService.GetExistentTagsAsync(ids);

        IDictionary<string, bool> dictExists = ids.ToDictionary(t => t, t => tags.Contains(t));

        return Logger.ExitMethod(dictExists);
    }

    /// <inheritdoc />
    public async Task<bool> CheckUserEmailExistsAsync(string email, string userId = "")
    {
        Logger.EnterMethod();

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
                        Operator = FilterOperator.EqualsCaseInsensitive
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

            Logger.LogDebugMessage("Found user with email '{email}'", LogHelpers.Arguments(email));

            return Logger.ExitMethod(duplicate);
        }

        Logger.LogDebugMessage("No user found with email '{email}'´.", LogHelpers.Arguments(email));

        return Logger.ExitMethod(false);
    }

    /// <inheritdoc />
    public async Task<bool> CheckGroupNameExistsAsync(
        string name,
        string displayName,
        bool ignoreCase,
        string groupId = "")
    {
        Logger.EnterMethod();

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
                            ? FilterOperator.EqualsCaseInsensitive
                            : FilterOperator.Equals
                    },
                    new Maverick.UserProfileService.Models.RequestModels.Definitions
                    {
                        FieldName = nameof(GroupBasic.DisplayName),
                        Values = new[] { name, displayName },
                        BinaryOperator = BinaryOperator.Or,
                        Operator = ignoreCase
                            ? FilterOperator.EqualsCaseInsensitive
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

            Logger.LogDebugMessage("Found group with name '{name}'", LogHelpers.Arguments(name));

            return Logger.ExitMethod(duplicate);
        }

        Logger.LogDebugMessage("No group found with name '{name}'.", LogHelpers.Arguments(name));

        return Logger.ExitMethod(false);
    }

    /// <inheritdoc cref="IProjectionReadService.GetProfileAsync" />
    public async Task<IProfile?> GetProfileAsync(string id, ProfileKind profileKind)
    {
        Logger.EnterMethod();

        try
        {
            var profile = await _readService.GetProfileAsync<IProfile>(
                id,
                profileKind.ConvertToRequestedProfileKind());

            Logger.LogDebugMessage(
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
        Logger.EnterMethod();

        IPaginatedList<IProfile> profiles =
            await _readService.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                ids,
                profileKind.ConvertToRequestedProfileKind());

        return Logger.ExitMethod(profiles);
    }

    /// <inheritdoc cref= "IProjectionReadService.GetProfilesAsync"/>
    public async Task<Tag?> GetTagAsync(string id)
    {
        Logger.EnterMethod();

        try
        {
            Tag tag = await _readService.GetTagAsync(id);

            return Logger.ExitMethod(tag);
        }
        catch (InstanceNotFoundException)
        {
            return Logger.ExitMethod<Tag?>(null);
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
        Logger.EnterMethod();

        JObject settings = await _readService.GetSettingsOfProfileAsync(profileId, profileKind, settingsKey);

        return Logger.ExitMethod(settings);
    }

    /// <inheritdoc cref="IProjectionReadService.GetFunctionAsync"/>
    public async Task<FunctionView?> GetFunctionAsync(string id)
    {
        Logger.EnterMethod();

        try
        {
            var function = await _readService.GetFunctionAsync<FunctionView>(id);

            return Logger.ExitMethod(function);
        }
        catch (InstanceNotFoundException)
        {
            return Logger.ExitMethod<FunctionView?>(null);
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<FunctionBasic>> GetFunctionsAsync(string roleId, string organizationId)
    {
        Logger.EnterMethod();

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

            return Logger.ExitMethod(functions);
        }
        catch (InstanceNotFoundException)
        {
            return Logger.ExitMethod<ICollection<FunctionBasic>>(new List<FunctionBasic>());
        }
    }

    /// <inheritdoc cref="IProjectionReadService.GetRoleAsync"/>
    public async Task<RoleBasic?> GetRoleAsync(string id)
    {
        Logger.EnterMethod();

        try
        {
            RoleView role = await _readService.GetRoleAsync(id);

            return Logger.ExitMethod(role);
        }
        catch (InstanceNotFoundException)
        {
            return Logger.ExitMethod<RoleBasic?>(null);
        }
    }

    /// <inheritdoc />
    public async Task<string[]> GetRoleFunctionAssignmentsAsync(string roleId)
    {
        Logger.EnterMethod();

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

        return Logger.ExitMethod(functionIds);
    }

    public async Task<IList<ConditionAssignment>> CheckExistingProfileAssignmentsAsync(
        string parentProfileId,
        ProfileKind profileKind,
        IList<ConditionObjectIdent> assignmentsToCheck,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        IList<ConditionAssignment> foundProfileAssignments = await _readService
            .GetDirectMembersOfContainerProfileAsync(
                parentProfileId,
                profileKind,
                assignmentsToCheck.Select(a => a.Id),
                cancellationToken)
            ?? new List<ConditionAssignment>();

        List<ConditionAssignment> flatListMissingItems = assignmentsToCheck
            .AsFlatAssignmentList()
            .Except(foundProfileAssignments.AsFlatList(),
                new OnlyFirstInSecondConditionAssignmentEqualityComparer())
            .ToList();

        return Logger.ExitMethod(flatListMissingItems);
    }

    /// <inheritdoc />
    public Task<string[]> GetAllParentsOfProfile(string id)
    {
        // Todo: Valid implementation missing - due of read service changes this method was not working any more
        Logger.EnterMethod();

        return Logger.ExitMethod(Task.FromResult(Array.Empty<string>()));
    }

    /// <inheritdoc />
    public async Task<ICollection<ProfileIdent>> GetParentsOfProfileAsync(string id)
    {
        Logger.EnterMethod();

        try
        {
            IPaginatedList<IContainerProfile> parents = await _readService
                .GetParentsOfProfileAsync<GroupBasic, OrganizationBasic>(id, RequestedProfileKind.All);

            ProfileIdent[] parentIds = parents.Select(p => new ProfileIdent(p.Id, p.Kind)).ToArray();

            if (!parentIds.Any())
            {
                Logger.LogDebugMessage("No parents found for profile with id '{id}'", LogHelpers.Arguments(id));
            }

            return Logger.ExitMethod(parentIds);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(e, e.Message, LogHelpers.Arguments(id));

            return Logger.ExitMethod(new List<ProfileIdent>());
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<ProfileIdent>> GetChildrenOfProfileAsync(string id, ProfileKind profileKind)
    {
        Logger.EnterMethod();

        try
        {
            IPaginatedList<IProfile> children = await _readService
                .GetChildrenOfProfileAsync<UserBasic, GroupBasic, OrganizationBasic>(
                    id,
                    profileKind.ToContainerProfileType());

            ProfileIdent[] parentIds = children.Select(p => new ProfileIdent(p.Id, p.Kind)).ToArray();

            if (!parentIds.Any())
            {
                Logger.LogDebugMessage("No children found for profile with id '{id}'", LogHelpers.Arguments(id));
            }

            return Logger.ExitMethod(parentIds);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(e, e.Message, LogHelpers.Arguments(id));

            return Logger.ExitMethod(new List<ProfileIdent>());
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<Member>> GetAssignedProfilesAsync(string roleOrFunctionId)
    {
        Logger.EnterMethod();

        IPaginatedList<Member> assignedMembers = await _readService.GetAssignedProfiles(roleOrFunctionId);

        return Logger.ExitMethod(assignedMembers);
    }
}
