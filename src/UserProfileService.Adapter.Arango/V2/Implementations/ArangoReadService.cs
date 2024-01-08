using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

public class ArangoReadService : ArangoRepositoryBase, IReadService
{
    protected readonly string _collectionPrefix;
    private readonly IDbInitializer _dbInitializer;

    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    // useful in tests
    public ArangoReadService(
        IServiceProvider serviceProvider,
        IDbInitializer dbInitializer,
        ILogger<ArangoReadService> logger,
        string arangoDbClientName,
        string collectionPrefix) : this(serviceProvider, dbInitializer, logger)
    {
        ArangoDbClientName = arangoDbClientName;
        _collectionPrefix = collectionPrefix;

        ModelBuilderOptions = DefaultModelConstellation
                              .CreateNew(_collectionPrefix)
                              .ModelsInfo;
    }
    
    
    /// <summary>
    ///     Initializes a new instance of <see cref="ArangoReadService" />.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The service provider is needed to create an <see cref="IArangoDbClientFactory" /> that
    ///     manages <see cref="IArangoDbClient" />s.
    /// </param>
    /// <param name="dbInitializer">
    ///     The instance of an <see cref="IDbInitializer" /> that will be used to initialize the
    ///     database.
    /// </param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> that will accept logging messages of this instance.</param>
    [ActivatorUtilitiesConstructor]
    public ArangoReadService(
        IServiceProvider serviceProvider,
        IDbInitializer dbInitializer,
        ILogger<ArangoReadService> logger) : base(logger, serviceProvider)
    {
        _dbInitializer = dbInitializer;
        _collectionPrefix = WellKnownDatabaseKeys.CollectionPrefixUserProfileService;
        ArangoDbClientName = ArangoConstants.DatabaseClientNameUserProfileStorage;
        ModelBuilderOptions = DefaultModelConstellation
                              .CreateNew(_collectionPrefix)
                              .ModelsInfo;
    }

    /// <inheritdoc />
    public async Task<IProfile> GetProfileByIdOrExternalIdAsync<TUser, TGroup, TOrgUnit>(
        string idOrExternalId,
        CancellationToken cancellationToken = default)
        where TUser : IProfile
        where TGroup : IContainerProfile
        where TOrgUnit : IContainerProfile
    {
        Logger.EnterMethod();

        if (idOrExternalId == null)
        {
            throw new ArgumentNullException(nameof(idOrExternalId));
        }

        if (string.IsNullOrWhiteSpace(idOrExternalId))
        {
            throw new ArgumentException("idOrExternalId cannot be null or whitespace.", nameof(idOrExternalId));
        }

        Logger.LogDebugMessage(
            "The resulting type can be (depending on kind of found profile): TUser = '{typeof(TUser).Name}' or TGroup = '{typeof(TGroup).Name}' or TOrgUnit = '{typeof(TOrgUnit).Name}'",
            LogHelpers.Arguments(typeof(TUser).Name, typeof(TGroup).Name, typeof(TOrgUnit).Name));

        List<IProfileEntityModel> response = await ExecuteQueryAsync<IProfileEntityModel>(
            e => e
                .First(p => p.Id == idOrExternalId || p.ExternalIds.Any(id => id.Id == idOrExternalId))
                .Select(p => p)
                .Compile(CollectionScope.Query),
            true,
            false,
            cancellationToken);

        Logger.LogInfoMessage(
            "Found profile using id or external id {id}: {found}",
            Arguments(
                idOrExternalId,
                response?.Count(r => !string.IsNullOrWhiteSpace(r.Id)) == 1));

        return response?
            .FirstOrDefault()
            ?
            .ToSpecifiedProfileModel<TUser, TGroup, TOrgUnit>();
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<IProfile>> GetProfilesAsync<TUser, TGroup, TOrgUnit>(
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TUser = {UserName}; TGroup = {GroupName}; TOrgUnit = {OrganizationName}.",
            LogHelpers.Arguments(typeof(TUser).Name, typeof(TGroup).Name, typeof(TOrgUnit).Name));

        options.Validate(Logger);

        PaginationApiResponse<IProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IProfileEntityModel>(
                f => f
                    .UsingOptions(options, expectedKind)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogInfoMessage(
            "Found {responseTotalCount} profiles. {responseQueryResultCount} profiles in result collection.",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response
                .QueryResult
                .ToSpecifiedProfileModels<TUser, TGroup, TOrgUnit>(options?.IncludeInactiveAssignments ?? true)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<IProfile>> GetProfilesWithTagAsync<TUser, TGroup, TOrgUnit>(
        string tag,
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        QueryObject options = null,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TUser = {UserName}; TGroup = {GroupName}; TOrgUnit = {OrganizationName}.",
            LogHelpers.Arguments(typeof(TUser).Name, typeof(TGroup).Name, typeof(TOrgUnit).Name));

        options.Validate(Logger);

        PaginationApiResponse<IProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IProfileEntityModel>(
                f => f
                    .Where(p => p.Tags.Any(t => t.Name == tag))
                    .UsingOptions(options, expectedKind)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogInfoMessage(
            "Found {responseTotalCount} profiles. {responseQueryResultCount} profiles in result collection.",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response
                .QueryResult
                .ToSpecifiedProfileModels<TUser, TGroup, TOrgUnit>()
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<IProfile>> GetProfilesAsync<TUser, TGroup, TOrgUnit>(
        IEnumerable<string> profileIds,
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic where TGroup : GroupBasic where TOrgUnit : OrganizationBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TUser = {userName}; TGroup = {groupName}",
            LogHelpers.Arguments(typeof(TUser).Name, typeof(TGroup).Name));

        List<string> profilesToBeUsed = profileIds?
            .Where(profileId => !string.IsNullOrWhiteSpace(profileId))
            .ToList();

        ValidationHelper.CheckIfParameterIsNullOrEmpty(profilesToBeUsed, nameof(profileIds));

        // tagsToBeUsed cannot be null
        // ReSharper disable AssignNullToNotNullAttribute
        Logger.LogDebugMessage(
            "Got profile ids: {profiles}",
            Arguments(string.Join(";", profilesToBeUsed)));

        PaginationApiResponse<IProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IProfileEntityModel>(
                f => f
                    .Where(p => profilesToBeUsed.Contains(p.Id))
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogInfoMessage(
            "Found {responseTotalAmount} profiles ({responseQueryResultCount} profiles in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response
                .QueryResult
                .ToSpecifiedProfileModels<TUser, TGroup, TOrgUnit>()
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<TProfile> GetProfileAsync<TProfile>(
        string profileId,
        RequestedProfileKind expectedKind,
        bool includeInactiveAssignments = true,
        CancellationToken cancellationToken = default)
        where TProfile : IProfile
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting type will be: TProfile = {profileName}.",
            LogHelpers.Arguments(typeof(TProfile).Name));

        List<IProfileEntityModel> response =
            await ExecuteQueryAsync<IProfileEntityModel, IProfileEntityModel>(
                query => query
                    .Where(expectedKind)
                    .First(p => p.Id == profileId)
                    .Select(p => p)
                    .Compile(CollectionScope.Query),
                true,
                true,
                cancellationToken);

        if (response == null || response.Count(item => item != null) == 0)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No {expectedKind switch { RequestedProfileKind.User => "user", RequestedProfileKind.Group => "group", _ => "profile" }} found with id '{profileId}'.");
        }

        Logger.LogDebugMessage("Found profile with id {profileId}.", LogHelpers.Arguments(profileId));

        return Logger.ExitMethod(
            response
                .FirstOrDefault(item => item != null)
                .ToSpecifiedProfileModel<TProfile>(includeInactiveAssignments));
    }
    
    /// <inheritdoc />
    public async Task<IPaginatedList<IContainerProfile>> GetRootProfilesAsync<TGroup, TOrgUnit>(
        RequestedProfileKind expectedKind,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TGroup = {groupName}; TOrgUnit = {organizationName}",
            Arguments(typeof(TGroup).Name, typeof(TOrgUnit).Name));

        Logger.LogDebugMessage(
            "The expected profile kind: {kind}",
            Arguments(expectedKind.ToString("G")));

        options.Validate(Logger);

        PaginationApiResponse<IContainerProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IContainerProfileEntityModel>(
                query => query
                    .Where(expectedKind)
                    // following method will contain logic which members are valid for expected profile kind
                    .WhereMemberOfIsEmptyValidFor(expectedKind)
                    .UsingOptions(options)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} root container profiles in total ({responseQueryResultCount} profiles in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response.QueryResult
                .ToSpecifiedContainerProfileModels<TGroup, TOrgUnit>(options?.IncludeInactiveAssignments ?? true)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<IContainerProfile>> GetParentsOfProfileAsync<TGroup, TOrgUnit>(
        string profileId,
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TGroup = {groupName}; TOrgUnit = {organizationName}",
            Arguments(typeof(TGroup).Name, typeof(TOrgUnit).Name));

        Logger.LogDebugMessage(
            "The expected profile kind: {expectedKind}",
            Arguments(expectedKind.ToString("G")));

        ValidationHelper.CheckParameter(profileId, nameof(profileId));

        options.Validate(Logger);

        List<string> existing = await ExecuteQueryAsync<IProfileEntityModel, string>(
            query => query
                .Where(expectedKind)
                .First(g => g.Id == profileId)
                .Select(g => g.Id)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        string specifiedProfileId =
            TrimQuotationMarkOnce(existing.FirstOrDefault(item => !string.IsNullOrEmpty(item)));

        if (string.IsNullOrWhiteSpace(specifiedProfileId))
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No {expectedKind.GetOutputString()} found with id '{profileId}'.");
        }

        Logger.LogDebugMessage(
            "Found {expectedKind} with id {{profileId}}.",
            Arguments(expectedKind.GetOutputString(), profileId));

        bool includeInactiveAssignments = options?.IncludeInactiveAssignments ?? true;

        PaginationApiResponse<IContainerProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IContainerProfileEntityModel>(
                query => query
                    .Where(expectedKind.ConvertToParentRequestedProfileKind())
                    .Where(
                        g => g.Members.Count(
                                m => (m.IsActive || includeInactiveAssignments) && m.Id == specifiedProfileId)
                            > 0)
                    .UsingOptions(options)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        return Logger.ExitMethod(
            AdjustConditionsOfParent(response.QueryResult, profileId)
                .ToSpecifiedContainerProfileModels<TGroup, TOrgUnit>(includeInactiveAssignments)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<List<IContainerProfile>> GetAllParentsOfProfileAsync(
        string profileId,
        RequestedProfileKind expectedKind,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(profileId, nameof(profileId));

        string edgeCollection = DefaultModelConstellation.CreateNew(_collectionPrefix)
            .ModelsInfo
            .GetRelation<IProfileEntityModel, GroupEntityModel>()
            .EdgeCollection;

        string entityCollection = DefaultModelConstellation.CreateNew(_collectionPrefix)
            .ModelsInfo
            .GetCollectionName<IProfile>();

        ParameterizedAql queryString = WellKnownAqlQueries.RetrieveAllOutboundEdges(
            edgeCollection,
            entityCollection,
            profileId,
            WellKnownAqlQueries.MaxTraversalDepth,
            v => WellKnownAqlQueries.GetDefaultFilterString(expectedKind, v));

        List<IContainerProfile> profiles =
            await ExecuteRawQueriesAsync<IContainerProfile>(
                queryString.Query,
                queryString.Parameter,
                true,
                true,
                cancellationToken: cancellationToken);

        Logger.LogDebugMessage(
            "Found {profilesCount} parents for profile with id {profileId}.",
            LogHelpers.Arguments(profiles.Count, profileId));

        return Logger.ExitMethod(profiles);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<IProfile>> GetChildrenOfProfileAsync<TUser, TGroup, TOrgUnit>(
        string profileId,
        ProfileContainerType expectedParentType,
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TUser = {TUser}; TGroup = {TGroup}; TOrgUnit = {TOrgUnit}",
            Arguments(typeof(TUser).Name, typeof(TGroup).Name, typeof(TOrgUnit).Name));

        Logger.LogDebugMessage(
            "The expected profile kind values are set: parent: {parent}; children: {children}",
            Arguments(expectedParentType.ToString("G"), expectedKind.ToString("G")));

        ValidationHelper.CheckParameter(profileId, nameof(profileId));

        options.Validate(Logger);

        string existing = TrimQuotationMarkOnce(
            (
                await ExecuteQueryAsync<IContainerProfileEntityModel, string>(
                    query => query
                        .Where(expectedParentType.Convert())
                        .First(g => g.Id == profileId)
                        .Select(g => g.Id)
                        .Compile(CollectionScope.Query),
                    true,
                    true,
                    cancellationToken)
            )?
            .FirstOrDefault());

        if (string.IsNullOrWhiteSpace(existing))
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No {expectedParentType.GetOutputString()} found with id {profileId}.");
        }

        if (expectedKind == RequestedProfileKind.Undefined)
        {
            expectedKind = RequestedProfileKind.All;
        }

        RequestedProfileKind combinedChildrenKindFilter =
            expectedParentType.ConvertToChildrenRequestedProfileKind() & expectedKind;

        if (combinedChildrenKindFilter == RequestedProfileKind.Undefined)
        {
            Logger.LogDebugMessage(
                "No supported profile kind was requested as child for container type {containerType}. Returning empty result set.",
                Arguments(expectedParentType.ToString("G")));

            return Logger.ExitMethod(new PaginatedList<IProfile>(Enumerable.Empty<IProfile>(), 0));
        }

        bool includeInactiveAssignments = options?.IncludeInactiveAssignments ?? true;

        PaginationApiResponse<IProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IProfileEntityModel>(
                query => query
                    .Where(combinedChildrenKindFilter)
                    .Where(
                        g => g.MemberOf.Count(m => (m.IsActive || includeInactiveAssignments) && m.Id == existing)
                            > 0)
                    .UsingOptions(options, expectedKind)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {totalAmount} profiles in total {queryResultCount} profiles in result collection.",
            Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            AdjustConditionsOfChildren(response.QueryResult, profileId)
                .ToSpecifiedProfileModels<TUser, TGroup, TOrgUnit>(includeInactiveAssignments)
                .ToPaginatedList(response.TotalAmount));
    }
    
    /// <inheritdoc />
    public Task<IPaginatedList<string>> GetFunctionalAccessRightsOfProfileAsync(
        string profileId,
        bool includeInherited = false,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> CheckFunctionalAccessRightOfProfileAsync(
        string profileId,
        string functionalName,
        bool breakInheritance = false,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IPaginatedList<IAssignmentObject>> GetLinksForProfileAsync(
        string profileId,
        QueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IList<ConditionAssignment>> GetDirectMembersOfContainerProfileAsync(
        string parentId,
        ProfileKind parentProfileKind,
        IEnumerable<string> memberIdFilter = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(parentId, nameof(parentId));
        ValidationHelper.CheckIfProfileIsContainer(parentProfileKind, nameof(parentProfileKind));

        string profileCollection = DefaultModelConstellation.CreateNew(_collectionPrefix)
            .ModelsInfo
            .GetCollectionName<IContainerProfile>();

        ParameterizedAql queryString = WellKnownAqlQueries.GetMembersOfProfileFilteredByMemberIds(
            profileCollection,
            parentId,
            parentProfileKind,
            memberIdFilter?.ToArray());

        var parentMissing = false;

        List<ConditionAssignment> members =
            await ExecuteRawQueriesAsync<ConditionAssignment>(
                queryString.Query,
                queryString.Parameter,
                true,
                true,
                warning => parentMissing = warning.Any(w => 
                    w.Contains($"[{ArangoRepoErrorCodes.ProfileNotFound}]")),
                cancellationToken);

        if (parentMissing)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"Parent entity missing (id: {parentId}; kind {parentProfileKind:G})",
                parentId);
        }

        return members;
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<Member>> GetAssignedProfiles(
        string roleOrFunctionId,
        QueryObject queryObject = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(roleOrFunctionId, nameof(roleOrFunctionId));

        queryObject?.Validate();

        List<string> foundObjects = await ExecuteQueryAsync<IAssignmentObjectEntity, string>(
            query => query
                .First(g => g.Id == roleOrFunctionId)
                .Select(o => o.Id)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        string internalId = TrimQuotationMarkOnce(foundObjects.FirstOrDefault(item => item != null));

        if (string.IsNullOrWhiteSpace(internalId))
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.RoleOrFunctionNotFoundString,
                $"No role or function found with id '{roleOrFunctionId}'.");
        }

        Logger.LogDebugMessage(
            "Found the role/function with id {roleOrFunctionId} (internal type: {internalId}).",
            LogHelpers.Arguments(roleOrFunctionId, internalId.Split('/').FirstOrDefault()));

        PaginationApiResponse<IProfileEntityModel> response =
            await ExecuteCountingQueriesAsync<IProfileEntityModel, IProfileEntityModel>(
                query => query
                    .Where(p => p.SecurityAssignments.Any(assignment => assignment.Id == internalId))
                    .UsingOptions(queryObject)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} roles in total ({responseQueryResultCount} roles in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            AdjustConditionsOfAssignments(response.QueryResult, roleOrFunctionId)
                .ToMemberModels()
                .Select(AdjustMembers)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<TRole>> GetRolesAsync<TRole>(
        QueryObject options = null,
        CancellationToken cancellationToken = default)
        where TRole : RoleBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting type will be: TRole = {roleName}.",
            LogHelpers.Arguments(typeof(TRole).Name));

        options.Validate(Logger);

        PaginationApiResponse<TRole> response = await ExecuteCountingQueriesAsync<RoleObjectEntityModel, TRole>(
            query => query
                .CastAndResolveProperties<RoleObjectEntityModel, TRole>(typeof(RoleView))
                .UsingOptions(options)
                .Compile(CollectionScope.Query),
            cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} roles in total ({responseQueryResultCount} roles in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response.QueryResult
                .Select(AdjustRole)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<RoleView> GetRoleAsync(
        string roleId,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(roleId, nameof(roleId));

        List<RoleObjectEntityModel> response = await ExecuteQueryAsync<RoleObjectEntityModel>(
            query => query
                .First(g => g.Id == roleId)
                .Select(r => r)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        if (response == null || response.Count(item => item != null) == 0)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No role found with id {roleId}.");
        }

        Logger.LogDebugMessage("Found the role with id {roleId}.", LogHelpers.Arguments(roleId));

        return Logger.ExitMethod(
            AdjustAssignmentObject(
                response.First(item => item != null)
                    .ToSpecifiedRoleModel<RoleView>()));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<TFunction>> GetFunctionsAsync<TFunction>(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TFunction : FunctionBasic
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TFunction = {functionName}.",
            LogHelpers.Arguments(typeof(TFunction).Name));

        options.Validate(Logger);

        PaginationApiResponse<TFunction> response =
            await ExecuteCountingQueriesAsync<FunctionObjectEntityModel, TFunction>(
                query => query
                    .CastAndResolveProperties<FunctionObjectEntityModel, TFunction>(typeof(FunctionView))
                    .UsingOptions(options)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} functions in total ({responseQueryResultCount} functions in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response.QueryResult
                .Select(AdjustFunction)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<TFunction> GetFunctionAsync<TFunction>(
        string functionId,
        CancellationToken cancellationToken = default)
        where TFunction : FunctionView
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting type will be: TFunction = {functionName}.",
            LogHelpers.Arguments(typeof(TFunction).Name));

        ValidationHelper.CheckParameter(functionId, nameof(functionId));

        List<FunctionObjectEntityModel> response = await ExecuteQueryAsync<FunctionObjectEntityModel>(
            query => query
                .First(g => g.Id == functionId)
                .Select(f => f)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        if (response == null || response.Count(item => item != null) == 0)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No function found with id '{functionId}'.");
        }

        Logger.LogDebugMessage("Found the function with id {functionId}.", LogHelpers.Arguments(functionId));

        return Logger.ExitMethod(
            AdjustAssignmentObject(
                AdjustFunction(
                    response
                        .First(item => item != null)
                        .ToSpecifiedFunctionModel<TFunction>())));
    }

    /// <inheritdoc />
    public Task<IPaginatedList<CalculatedTag>> GetTagsOfProfileAsync(
        string profileOrObjectId,
        RequestedTagType tagType,
        CancellationToken cancellationToken = default)
    {
        ValidationHelper.CheckParameter(profileOrObjectId, nameof(profileOrObjectId));

        return GetTagsOfProfileInternalAsync(profileOrObjectId, tagType.ConvertToTagType(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<LinkedRoleObject>> GetRolesOfProfileAsync(
        string profileId,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(profileId, nameof(profileId));

        options.Validate(Logger);

        List<string> existing = await ExecuteQueryAsync<IProfileEntityModel, string>(
            query => query
                .First(p => p.Id == profileId)
                .Select(p => p.SystemId)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        if (existing == null || existing.All(string.IsNullOrEmpty))
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No profile found with id {profileId}.");
        }

        bool includeInactiveAssignments = options?.IncludeInactiveAssignments ?? true;

        PaginationApiResponse<RoleObjectEntityModel> response =
            await ExecuteCountingQueriesAsync<RoleObjectEntityModel, RoleObjectEntityModel>(
                query => query
                    .Where(
                        r => r.LinkedProfiles.Count(
                                p => (p.IsActive || includeInactiveAssignments) && p.Id == profileId)
                            < 0)
                    .UsingOptions(options)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} roles related to {profileId} in total ({responseQueryResultCount} roles in result collection).",
            LogHelpers.Arguments(response.TotalAmount, profileId, response.QueryResult.Count));

        return Logger.ExitMethod(
            AdjustConditionsOfAssignmentObject(response.QueryResult, profileId)
                .Select(ConversionUtilities.ToConditionalRole)
                .Select(AdjustConditionalRole)
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<LinkedFunctionObject>> GetFunctionsOfProfileAsync(
        string profileId,
        bool returnFunctionsRecursively = false,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (returnFunctionsRecursively)
        {
            Logger.ExitMethod();

            return await GetFunctionsRecursivelyForUserAsync(profileId, cancellationToken);
        }

        ValidationHelper.CheckParameter(profileId, nameof(profileId));

        options.Validate(Logger);

        // just to check, if the referencing profile exists
        await CheckSystemIdOfProfileExistsAsync(profileId, ProfileKind.Unknown, cancellationToken);

        bool includeInactiveAssignments = options?.IncludeInactiveAssignments ?? true;

        PaginationApiResponse<FunctionObjectEntityModel> response =
            await ExecuteCountingQueriesAsync<FunctionObjectEntityModel, FunctionObjectEntityModel>(
                query => query
                    .Where(
                        r => r.LinkedProfiles.Count(
                                p => (p.IsActive || includeInactiveAssignments) && p.Id == profileId)
                            > 0)
                    .UsingOptions(options)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} functions related to {profileId} in total ({responseQueryResultCount} functions in result collection).",
            LogHelpers.Arguments(response.TotalAmount, profileId, response.QueryResult.Count));

        return Logger.ExitMethod(
            AdjustConditionsOfAssignmentObject(response.QueryResult, profileId)
                .Select(ConversionUtilities.ToConditionalFunction)
                .Select(AdjustConditionalFunction)
                .ToPaginatedList(response.TotalAmount));
    }

    internal async Task<IPaginatedList<LinkedFunctionObject>> GetFunctionsRecursivelyForUserAsync(
        string profileId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(profileId, nameof(profileId));

        await CheckSystemIdOfProfileExistsAsync(profileId, ProfileKind.User, cancellationToken);

        Logger.LogInfoMessage(
            "Starting to retrieve active functions-assignments recursively for user : {userId}.",
            profileId.ToLogString().AsArgumentList());

        List<List<ObjectIdent>> activeMemberShipsResponse =
            await ExecuteQueryAsync<SecondLevelProjectionAssignmentsUser, List<ObjectIdent>>(
                query => query
                    ?.First(p => p.ProfileId == profileId)
                    .Select(p => p.ActiveMemberships)
                    .Compile(CollectionScope.Query),
                true,
                true,
                cancellationToken);

        List<string> listOfActiveRecursiveFunctions =
            activeMemberShipsResponse?.FirstOrDefault()
                ?.Where(am => am.Type == ObjectType.Function)
                .Select(p => p.Id)
                .ToList();

        if (listOfActiveRecursiveFunctions == null || listOfActiveRecursiveFunctions.Count == 0)
        {
            return new PaginatedList<LinkedFunctionObject>();
        }

        Logger.LogInfoMessage(
            "Found {countActiveFunctionAssignments} for the user {userId}.",
            LogHelpers.Arguments(listOfActiveRecursiveFunctions.Count, profileId.ToLogString()));

        Logger.LogDebugMessage(
            "Found active functionIds: {listOfFunctionIds} for user {userIds}",
            LogHelpers.Arguments(listOfActiveRecursiveFunctions.ToLogString(), profileId.ToLogString()));

        PaginationApiResponse<FunctionBasic> functionResponse =
            await ExecuteCountingQueriesAsync<FunctionBasic, FunctionBasic>(
                query => query.Where(p => listOfActiveRecursiveFunctions.Contains(p.Id))
                    .Select(p => p)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Found functions for user {userId}: {allFunctions}",
                LogHelpers.Arguments(profileId.ToLogString(), functionResponse.QueryResult.ToLogString()));
        }

        return Logger.ExitMethod(
            functionResponse.QueryResult.Select(ConversionUtilities.ToConditionalFunction)
                // Done because we return only active function assignments
                // Inactive assignment does not make sense, because you need
                // the whole tree to know which condition is inactive and has
                // to become true, so that the user get the function.
                .Select(ConversionUtilities.SetIsActiveToTrue)
                .ToPaginatedList(functionResponse.TotalAmount));
    }

    /// <inheritdoc cref="IReadService" />
    public async Task<JObject> GetSettingsOfProfileAsync(
        string profileId,
        ProfileKind profileKind,
        string settingsKey,
        bool includeInherited = true,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(profileId, nameof(profileId));
        ValidationHelper.CheckParameter(settingsKey, nameof(settingsKey));

        Logger.LogDebugMessage(
            "Input parameter: profile id: {profileId}; kind {profileKind}; settings key: {settingsKey}.",
            Arguments(profileId, profileKind, settingsKey));

        PaginationApiResponse<ClientSettingsEntityModel> response =
            await ExecuteCountingQueriesAsync<ClientSettingsEntityModel>(
                query => query
                    .Where(obj => obj.ProfileId == profileId && obj.SettingsKey == settingsKey)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        JObject settings = response?
            .QueryResult
            .FirstOrDefault(s => includeInherited || !s.IsInherited)
            ?
            .Value;

        if (settings is not
            {
                HasValues: true
            })
        {
            Logger.LogDebugMessage(
                "Did not find any config key {settingsKey} for profile id {profileId}.",
                Arguments(settingsKey, profileId));
        }
        else
        {
            Logger.LogDebugMessage(
                "Found config key {settingsKey} for profile id {profileId}: {response}",
                Arguments(settingsKey, profileId, settings.ToString(Formatting.None)));
        }

        return Logger.ExitMethod(settings);
    }

    /// <exception cref="InstanceNotFoundException">
    ///     If the profile id could not be found or the related profile is not of the
    ///     requested kind..
    /// </exception>
    private async Task CheckSystemIdOfProfileExistsAsync(
        string profileId,
        ProfileKind profileKind,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        bool isProfileKindUnknown = profileKind == ProfileKind.Unknown;

        List<string> existing = await ExecuteQueryAsync<IProfileEntityModel, string>(
            query => query
                .First(p => p.Id == profileId && (isProfileKindUnknown || p.Kind == profileKind))
                .Select(p => p.SystemId)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        string foundSystemId = existing?.FirstOrDefault(item => !string.IsNullOrEmpty(item));

        if (string.IsNullOrWhiteSpace(foundSystemId))
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No {profileKind.Convert().GetOutputString()} found with id {profileId}.");
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<IList<ObjectIdent>> GetAllAssignedIdsOfUserAsync(
        string userId,
        bool includeInactiveAssignments,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Logger.LogTraceMessage(
            "Parameters: userId: {userId}; includeInactiveAssignments: {includeInactiveAssignments}",
            LogHelpers.Arguments(userId, includeInactiveAssignments));

        List<string> profileResponse =
            await ExecuteQueryAsync<UserEntityModel, string>(
                query => query
                    .First(p => p.Id == userId)
                    .Select(p => p.Id)
                    .Compile(CollectionScope.Query),
                true,
                true,
                cancellationToken);

        if (profileResponse == null || profileResponse.Count(item => item != null) == 0)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No user profile found with id '{userId}'.");
        }

        if (!includeInactiveAssignments)
        {
            List<List<ObjectIdent>> activeAssignmentsResponse =
                await ExecuteQueryAsync<SecondLevelProjectionAssignmentsUser, List<ObjectIdent>>(
                    query => query
                        .First(u => u.ProfileId == userId)
                        .Select(o => o.ActiveMemberships)
                        .Compile(CollectionScope.Query),
                    true,
                    true,
                    cancellationToken);

            List<ObjectIdent> activeAssignments = activeAssignmentsResponse?
                    .FirstOrDefault()
                ?? new List<ObjectIdent>();

            Logger.LogInfoMessage(
                "Active assignments for user {userId} found: {amountActiveAssignments} elements",
                LogHelpers.Arguments(userId, activeAssignments.Count));

            return Logger.ExitMethod(activeAssignments);
        }

        List<List<SecondLevelProjectionAssignment>> assignmentsResponse =
            await ExecuteQueryAsync<SecondLevelProjectionAssignmentsUser, List<SecondLevelProjectionAssignment>>(
                query => query
                    .First(u => u.ProfileId == userId)
                    .Select(o => o.Assignments)
                    .Compile(CollectionScope.Query),
                true,
                true,
                cancellationToken);

        List<ObjectIdent> assignments =
            assignmentsResponse?
                .FirstOrDefault()
                ?
                .Select(entry => entry.Parent)
                .Where(ident => ident != null)
                .ToList()
            ?? new List<ObjectIdent>();

        Logger.LogInfoMessage(
            "Assignments for user {userId} found: {amountAssignments} elements",
            LogHelpers.Arguments(userId, assignments.Count));

        return Logger.ExitMethod(assignments);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<Tag>> GetTagsAsync(
        QueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        options.Validate(Logger);

        PaginationApiResponse<Tag> tags = await ExecuteCountingQueriesAsync<Tag>(
            query => query
                .UsingOptions(options)
                .Compile(CollectionScope.Query),
            cancellationToken);

        Logger.LogDebugMessage(
            "Found {count} tags (total amount: {totalAmount}).",
            Arguments(tags.QueryResult.Count, tags.TotalAmount));

        return Logger.ExitMethod(
            tags
                .QueryResult
                .ToPaginatedList(tags.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<Tag> GetTagAsync(string tagId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ValidationHelper.CheckParameter(tagId, nameof(tagId));

        List<Tag> response =
            await ExecuteQueryAsync<Tag>(
                query => query
                    .First(p => p.Id == tagId)
                    .Select(p => p)
                    .Compile(CollectionScope.Query),
                true,
                false,
                cancellationToken);

        Tag foundTag = response?.SingleOrDefault(r => !string.IsNullOrWhiteSpace(r?.Id));

        if (foundTag == null)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.TagNotFoundString,
                $"No tag found with id {tagId}.");
        }

        Logger.LogDebugMessage(
            "Found tag with id {tagId}: Name = {tagName}; Type = {tagType}",
            Arguments(
                foundTag.Id,
                foundTag.Name,
                foundTag.Type.ToString("G")));

        return Logger.ExitMethod(foundTag);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<Tag>> GetTagsAsync(
        IEnumerable<string> tagIds,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        List<string> tagsToBeUsed = tagIds?
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToList();

        ValidationHelper.CheckIfParameterIsNullOrEmpty(tagsToBeUsed, nameof(tagIds));

        // tagsToBeUsed cannot be null
        // ReSharper disable AssignNullToNotNullAttribute
        Logger.LogDebugMessage(
            "Got tag ids: {tags}",
            Arguments(string.Join(";", tagsToBeUsed)));

        PaginationApiResponse<Tag> tags =
            await ExecuteCountingQueriesAsync<Tag>(
                query => query
                    .Where(tag => tagsToBeUsed.Contains(tag.Id))
                    .Select(p => p)
                    .Compile(CollectionScope.Query),
                cancellationToken,
                false,
                false);

        Logger.LogDebugMessage(
            "Found {count} tags (total amount: {totalAmount}).",
            Arguments(tags.QueryResult.Count, tags.TotalAmount));

        return Logger.ExitMethod(
            tags
                .QueryResult
                .ToPaginatedList(tags.TotalAmount));
    }

    public async Task<List<IProfile>> GetProfileByExternalOrInternalIdAsync<TUser, TGroup, TOrgUnit>(
        string profileId,
        bool allowExternalIds = true,
        string source = null,
        CancellationToken cancellationToken = default)
        where TUser : User
        where TGroup : Group
        where TOrgUnit : Organization
    {
        Logger.EnterMethod();

        ValidationHelper.CheckIfParameterIsNullOrEmpty(profileId, nameof(profileId));

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "ProfileId: {profileId} , allowExternalId:{allowExternalIds}, source: {source}",
                LogHelpers.Arguments(
                    profileId.ToLogString(),
                    allowExternalIds.ToLogString(),
                    source.ToLogString()));
        }

        string entityCollection = DefaultModelConstellation.CreateNew(_collectionPrefix)
            .ModelsInfo
            .GetQueryCollectionName<IProfileEntityModel>();

        ParameterizedAql retrieveProfileAql = WellKnownAqlQueries.RetrieveProfileByExternalOrInternalId(
            entityCollection,
            profileId,
            source,
            allowExternalIds);

        List<IProfileEntityModel> result = await ExecuteRawQueriesAsync<IProfileEntityModel>(
            retrieveProfileAql.Query,
            retrieveProfileAql.Parameter,
            true,
            true,
            cancellationToken: cancellationToken);

        Logger.LogInfoMessage(
            "Found {profilesFound} {profile} for profileId: {profileId} ",
            LogHelpers.Arguments(
                result.Count,
                result.Count > 1 ? "profiles" : "profile",
                profileId.ToLogString()));

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The found {profile}: {profileList}",
                LogHelpers.Arguments(result.Count > 1 ? "profiles" : "profile", result.ToLogString()));
        }

        return Logger.ExitMethod(result.ToSpecifiedProfileModels<TUser, TGroup, TOrgUnit>().ToList());
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetExistentTagsAsync(
        IEnumerable<string> tagIds,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        List<string> tagsToBeUsed = tagIds?
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToList();

        ValidationHelper.CheckIfParameterIsNullOrEmpty(tagsToBeUsed, nameof(tagIds));

        // tagsToBeUsed cannot be null
        // ReSharper disable AssignNullToNotNullAttribute
        Logger.LogDebugMessage(
            "Got tag ids: {tags}",
            Arguments(string.Join(";", tagsToBeUsed)));

        List<string> tags = await ExecuteQueryAsync<Tag, string>(
            query => query
                .Where(tag => tagsToBeUsed.Contains(tag.Id))
                .Select(tag => tag.Id)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);
        // ReSharper restore AssignNullToNotNullAttribute

        Logger.LogDebugMessage(
            "Found {count} existent tags.",
            Arguments(tags.Count));

        return Logger.ExitMethod(TrimQuotationMarkOnce(tags));
    }

    /// <inheritdoc />
    public async Task<IList<IProfile>> GetAllProfilesAsync(
        RequestedProfileKind profileKindFilter = RequestedProfileKind.All,
        string sortingPropertyName = "Id",
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        List<IProfileEntityModel> list = await ExecuteQueryAsync<IProfileEntityModel>(
            query => query
                .Where(profileKindFilter)
                .SortBy(sortingPropertyName, sortOrder)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        Logger.LogInfoMessage("Found {count} profiles in total.", Arguments(list?.Count ?? 0));

        return Logger.ExitMethod(
            list?.ToSpecifiedProfileModels<UserView, Group, Organization>()?.ToList() ?? new List<IProfile>());
    }

    /// <inheritdoc />
    public async Task<IList<IAssignmentObject>> GetAllAssignmentObjectsAsync(
        RequestedAssignmentObjectType typeFilter = RequestedAssignmentObjectType.Function,
        string sortingPropertyName = "id",
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        List<IAssignmentObjectEntity> list = await ExecuteQueryAsync<IAssignmentObjectEntity>(
            query => query
                .Where(typeFilter)
                .SortBy(sortingPropertyName, sortOrder)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        Logger.LogInfoMessage("Found {count} assignment objects in total.", Arguments(list?.Count ?? 0));

        return Logger.ExitMethod(
            list?.ToSpecifiedModel<RoleView, FunctionView>()?.ToList() ?? new List<IAssignmentObject>());
    }

    private async Task<IPaginatedList<CalculatedTag>> GetTagsOfProfileInternalAsync(
        string profileId,
        TagType? tagType = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        List<IProfileEntityModel> profile = await ExecuteQueryAsync<IProfileEntityModel>(
            query => query
                .First(p => p.Id == profileId)
                .Select(p => p)
                .Compile(CollectionScope.Query),
            true,
            true,
            cancellationToken);

        if (profile.FirstOrDefault() == null)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"The profile {profileId} could not be found.");
        }

        List<CalculatedTag> calculatedTags = profile.First().Tags ?? new List<CalculatedTag>();

        if (tagType != null)
        {
            List<CalculatedTag> filteredTags = calculatedTags.Where(t => t.Type == tagType.Value).ToList();

            Logger.LogDebugMessage(
                "Found {profileTagCount} tags of type {tagType} for profile {profileId}.",
                LogHelpers.Arguments(filteredTags.Count, tagType.Value.ToString(), profileId));

            return Logger.ExitMethod(filteredTags.ToPaginatedList(filteredTags.Count));
        }

        Logger.LogDebugMessage(
            "Found {profileTagCount} tags for profile {profileId}.",
            LogHelpers.Arguments(calculatedTags.Count, profileId));

        return Logger.ExitMethod(
            calculatedTags
                .ToPaginatedList(calculatedTags.Count));
    }

    public Task<PaginationApiResponse<TEntity>> ExecuteCountingQueriesAsync<TEntity>(
        Func<IArangoDbEnumerable<TEntity>, IArangoDbQueryResult> selectionQuery,
        CancellationToken cancellationToken = default,
        bool throwException = true,
        bool throwExceptionIfNotFound = true,
        [CallerMemberName] string caller = null)
        where TEntity : class
    {
        return ExecuteCountingQueriesAsync<TEntity, TEntity>(
            selectionQuery,
            cancellationToken,
            throwException,
            throwExceptionIfNotFound,
            caller);
    }

    
    protected virtual ModelBuilderOptions ModelBuilderOptions { get; }
    
    protected async Task<PaginationApiResponse<TOutput>> ExecuteCountingQueriesAsync<TEntity, TOutput>(
        Func<IArangoDbEnumerable<TEntity>, IArangoDbQueryResult> selectionQuery,
        CancellationToken cancellationToken = default,
        bool throwException = true,
        bool throwExceptionIfNotFound = true,
        [CallerMemberName] string caller = null)
        where TEntity : class
        where TOutput : class
    {
        Logger.EnterMethod();

        IArangoDbQueryResult queryBuilderResult = selectionQuery.Invoke(
                ModelBuilderOptions
                .Entity<TEntity>());

        string countQueryString = queryBuilderResult.GetCountQueryString();
        string queryString = queryBuilderResult.GetQueryString();

        Logger.LogDebugMessage(
            "Using aql queries: To count: {count}. To select: {queryString}.",
            LogHelpers.Arguments(
                countQueryString,
                queryString));

        PaginationApiResponse<TOutput> response = await ExecuteAsync(
            async client =>
            {
                MultiApiResponse<CountingModel> counting =
                    await client
                        .ExecuteQueryWithCursorOptionsAsync
                            <CountingModel>(
                                new CreateCursorBody
                                {
                                    Query =
                                        countQueryString
                                },
                                cancellationToken: cancellationToken);

                MultiApiResponse<TOutput> selection =
                    await client
                        .ExecuteQueryWithCursorOptionsAsync
                            <TOutput>(
                                new CreateCursorBody
                                {
                                    Query = queryString
                                },
                                cancellationToken: cancellationToken);

                return new PaginationApiResponse<TOutput>(selection, counting);
            },
            throwException,
            throwExceptionIfNotFound,
            cancellationToken,
            caller);

        Logger.LogTraceMessage(
            "Executing queries: counting: {countingQuery}; selection: {query} in behalf of {caller}",
            Arguments(countQueryString, queryString, caller));

        return Logger.ExitMethod(response);
    }

    private async Task<List<TResult>> ExecuteRawQueriesAsync<TResult>(
        string rawSelectionAqlQueryString,
        Dictionary<string, object> bindVariables,
        bool throwException,
        bool throwExceptionIfNotFound,
        Action<List<string>> warningsListener = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
        where TResult : class
    {
        Logger.LogDebugMessage(
            "Using aql queries: Raw selection query: {rawSelectionAqlQueryString}.",
            LogHelpers.Arguments(rawSelectionAqlQueryString));

        MultiApiResponse<TResult> response = await ExecuteAsync(
            async client =>
                await client.ExecuteQueryWithCursorOptionsAsync<TResult>(
                    new CreateCursorBody
                    {
                        Query = rawSelectionAqlQueryString,
                        BindVars = bindVariables
                    },
                    cancellationToken: cancellationToken),
            throwException,
            throwExceptionIfNotFound,
            cancellationToken,
            caller);

        if (warningsListener != null 
            && response.Warnings is
            {
                Count: > 0
            })
        {
            warningsListener.Invoke(response.Warnings);
        }

        Logger.LogTraceMessage(
            "Executing query {query} in behalf of {caller}",
            Arguments(rawSelectionAqlQueryString, caller));

        return Logger.ExitMethod(response.QueryResult?.ToList() ?? new List<TResult>());
    }

    private Task<List<TResult>> ExecuteQueryAsync<TResult>(
        Func<IArangoDbEnumerable<TResult>, IArangoDbQueryResult> selectionQuery,
        bool throwException,
        bool throwExceptionIfNotFound,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
    {
        return ExecuteQueryAsync<TResult, TResult>(
            selectionQuery,
            throwException,
            throwExceptionIfNotFound,
            cancellationToken,
            caller);
    }

    public async Task<List<TOutput>> ExecuteQueryAsync<TEntity, TOutput>(
        Func<IArangoDbEnumerable<TEntity>, IArangoDbQueryResult> selectionQuery,
        bool throwException,
        bool throwExceptionIfNotFound,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
    {
        Logger.EnterMethod();

        IArangoDbQueryResult queryBuilderResult =
            selectionQuery.Invoke(DefaultModelConstellation.CreateNew(_collectionPrefix).ModelsInfo.Entity<TEntity>());

        Logger.LogDebugMessage(
            "Using aql queries: Selection: {queryString}.",
            LogHelpers.Arguments(queryBuilderResult.GetQueryString()));

        string query = queryBuilderResult.GetQueryString();

        MultiApiResponse<TOutput> response = await ExecuteAsync(
            client => client
                .ExecuteQueryWithCursorOptionsAsync<TOutput>(
                    new CreateCursorBody
                    {
                        Query = query
                    },
                    cancellationToken: cancellationToken),
            throwException,
            throwExceptionIfNotFound,
            cancellationToken,
            caller);

        Logger.LogTraceMessage(
            "Executing query {query} in behalf of {caller}",
            Arguments(query, caller));

        return Logger.ExitMethod(response.QueryResult?.ToList() ?? new List<TOutput>());
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IArangoDbClient, Task<TResult>> method,
        bool throwException,
        bool throwExceptionIfNotFound,
        CancellationToken cancellationToken,
        string caller)
        where TResult : IApiResponse
    {
        Logger.EnterMethod();

        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        TResult response = await SendRequestAsync(
            method,
            throwException,
            throwExceptionIfNotFound,
            CallingServiceContext.CreateNewOf<ArangoReadService>(),
            cancellationToken,
            caller);

        return Logger.ExitMethod(response);
    }

    private static IReadOnlyCollection<IProfileEntityModel> AdjustConditionsOfParent(
        IReadOnlyList<IContainerProfileEntityModel> collection,
        string childId)
    {
        if (collection == null)
        {
            return new List<IProfileEntityModel>();
        }

        foreach (IContainerProfileEntityModel profile in collection)
        {
            if (profile == null)
            {
                continue;
            }

            profile.Conditions = profile.Members?
                    .FirstOrDefault(m => m.Id == childId)
                    ?
                    .Conditions
                ?? new List<RangeCondition>();
        }

        return collection;
    }

    private static IReadOnlyCollection<IProfileEntityModel> AdjustConditionsOfChildren(
        IReadOnlyList<IProfileEntityModel> collection,
        string parentId)
    {
        if (collection == null)
        {
            return new List<IProfileEntityModel>();
        }

        foreach (IProfileEntityModel profile in collection)
        {
            if (profile == null)
            {
                continue;
            }

            profile.Conditions = profile.MemberOf?
                    .FirstOrDefault(m => m.Id == parentId)
                    ?
                    .Conditions
                ?? new List<RangeCondition>();
        }

        return collection;
    }

    private static IReadOnlyCollection<IProfileEntityModel> AdjustConditionsOfAssignments(
        IReadOnlyList<IProfileEntityModel> collection,
        string parentId)
    {
        if (collection == null)
        {
            return new List<IProfileEntityModel>();
        }

        foreach (IProfileEntityModel profile in collection)
        {
            if (profile == null)
            {
                continue;
            }

            profile.Conditions = profile.SecurityAssignments?
                    .FirstOrDefault(m => m.Id == parentId)
                    ?
                    .Conditions
                ?? new List<RangeCondition>();
        }

        return collection;
    }

    private static IReadOnlyCollection<TTarget> AdjustConditionsOfAssignmentObject<TTarget>(
        IReadOnlyList<TTarget> collection,
        string profileId) where TTarget : class, IAssignmentObjectEntity
    {
        if (collection == null)
        {
            return new List<TTarget>();
        }

        foreach (TTarget assignmentObject in collection)
        {
            if (assignmentObject == null)
            {
                continue;
            }

            assignmentObject.Conditions = assignmentObject.LinkedProfiles?
                    .FirstOrDefault(m => m.Id == profileId)
                    ?
                    .Conditions
                ?? new List<RangeCondition>();
        }

        return collection;
    }

    private static TFunc AdjustFunction<TFunc>(TFunc function)
        where TFunc : FunctionBasic
    {
        function.Organization = function.Organization.EnsureBasicOrganizationProfile();

        if (function is not FunctionView view)
        {
            return function;
        }

        view.LinkedProfiles ??= new List<Member>();

        return function;
    }

    private static LinkedFunctionObject AdjustConditionalFunction(LinkedFunctionObject function)
    {
        if (function is
            {
                Conditions: null
            })
        {
            function.Conditions = new List<RangeCondition>();
        }

        return function;
    }

    private static TRole AdjustRole<TRole>(TRole role)
        where TRole : RoleBasic
    {
        if (role is
            {
                Permissions: null
            })
        {
            role.Permissions = new List<string>();
        }

        if (role is RoleView view)
        {
            view.LinkedProfiles ??= new List<Member>();
        }

        return role;
    }

    private static LinkedRoleObject AdjustConditionalRole(LinkedRoleObject role)
    {
        if (role is
            {
                Conditions: null
            })
        {
            role.Conditions = new List<RangeCondition>();
        }

        return role;
    }

    private static Member AdjustMembers(Member member)
    {
        if (member is
            {
                Conditions: null
            })
        {
            member.Conditions = new List<RangeCondition>();
        }

        return member;
    }

    private static TAssignmentObject AdjustAssignmentObject<TAssignmentObject>(TAssignmentObject assignmentObject)
        where TAssignmentObject : IAssignmentObject
    {
        if (assignmentObject == null
            || assignmentObject.LinkedProfiles != null)
        {
            return assignmentObject;
        }

        assignmentObject.LinkedProfiles ??= new List<Member>();

        return assignmentObject;
    }
}
