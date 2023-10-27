using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.Models.EnumModels;
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
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Stores;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoSyncEntityStore : ArangoRepositoryBase, IEntityStore
{
    private readonly IDbInitializer _initializer;
    private readonly ModelBuilderOptions _modelsInfo;
    private readonly JsonSerializerSettings _serializerSettings;

    /// <inheritdoc cref="ArangoRepositoryBase.ArangoDbClientName" />
    protected override string ArangoDbClientName { get; }

    [ActivatorUtilitiesConstructor]
    public ArangoSyncEntityStore(
        ILogger<ArangoSyncEntityStore> logger,
        IServiceProvider serviceProvider,
        IJsonSerializerSettingsProvider jsonSerializerSettings,
        IDbInitializer initializer,
        string databaseClientName = null)
        : this(
            logger,
            serviceProvider,
            jsonSerializerSettings?.GetNewtonsoftSettings(),
            initializer,
            databaseClientName ?? ArangoConstants.DatabaseClientNameSync,
            WellKnownDatabaseKeys.CollectionPrefixSync)
    {
    }

    public ArangoSyncEntityStore(
        ILogger<ArangoSyncEntityStore> logger,
        IServiceProvider serviceProvider,
        JsonSerializerSettings serializerSettings,
        IDbInitializer initializer,
        string clientName,
        string collectionPrefix) : base(logger, serviceProvider)
    {
        ArangoDbClientName = clientName;
        _modelsInfo = DefaultModelConstellation.CreateSyncEntityStore(collectionPrefix).ModelsInfo;
        _serializerSettings = serializerSettings;
        _initializer = initializer;
    }

    private async Task<IPaginatedList<T>> GetProfilesAsync<T>(
        RequestedProfileKind expectedKind,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default) where T : class, ISyncProfile
    {
        Logger.EnterMethod();

        options.Validate(Logger);

        PaginationApiResponse<T> response =
            await ExecuteCountingQueriesAsync<T>(
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
                .ToPaginatedList(response.TotalAmount));
    }

    private async Task<CreateDocumentResponse> CreateEntityInternalAsync<TEntity>(
        TEntity entity,
        string entityId = null,
        bool withResponseCheck = true,
        CancellationToken cancellationToken = default)

    {
        Logger.EnterMethod();

        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentException("The entity Id should not be empty or whitespace", nameof(entityId));
        }

        string collectionName = _modelsInfo.GetCollectionName<TEntity>();

        Logger.LogDebugMessage(
            "Inserting entity of type :{entityType} with id: {profileId} in the collection {collectionName}.",
            LogHelpers.Arguments(typeof(TEntity), entityId, collectionName));

        CreateDocumentResponse response;

        try
        {
            response = await GetArangoDbClient()
                .CreateDocumentAsync(
                    collectionName,
                    entity.InjectDocumentKey(_ => entityId, _serializerSettings),
                    new CreateDocumentOptions
                    {
                        Overwrite = false,
                        ReturnNew = true,
                        ReturnOld = true,
                        OverWriteMode = AOverwriteMode.Conflict,
                        WaitForSync = true
                    });
        }
        catch (Exception ex)
        {
            Logger.LogErrorMessage(
                ex,
                "Error happened by inserting entity of type :{entityType} with id: {profileId} in the collection {collectionName}.",
                LogHelpers.Arguments(typeof(TEntity), entityId, collectionName));

            throw;
        }

        if (response == null)
        {
            Logger.LogWarnMessage(
                "Unknown Error happened by inserting entity of type :{entityType} with id: {profileId} in the collection {collectionName}: The database returned null",
                LogHelpers.Arguments(typeof(TEntity), entityId, collectionName));

            return Logger.ExitMethod(default(CreateDocumentResponse));
        }

        if (!response.Error)
        {
            Logger.LogInfoMessage(
                "Entity of type: {entityType} with the id:{entityId} has been created in the collection: {collectionName}",
                LogHelpers.Arguments(typeof(TEntity), entityId, collectionName));
        }

        if (withResponseCheck)
        {
            await CheckAResponseAsync(
                    response,
                    true,
                    context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return Logger.ExitMethod(response);
    }

    private Task<PaginationApiResponse<TEntity>> ExecuteCountingQueriesAsync<TEntity>(
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

    private async Task<PaginationApiResponse<TOutput>> ExecuteCountingQueriesAsync<TEntity, TOutput>(
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
            _modelsInfo
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

                return Logger.ExitMethod(new PaginationApiResponse<TOutput>(selection, counting));
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

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IArangoDbClient, Task<TResult>> method,
        bool throwException,
        bool throwExceptionIfNotFound,
        CancellationToken cancellationToken,
        string caller)
        where TResult : IApiResponse
    {
        Logger.EnterMethod();

        await _initializer.EnsureDatabaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        TResult response = await SendRequestAsync(
            method,
            throwException,
            throwExceptionIfNotFound,
            CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken,
            caller);

        return Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<UserSync>> GetUsersAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        IPaginatedList<UserSync> result = await GetProfilesAsync<UserSync>(
            RequestedProfileKind.User,
            options,
            cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<GroupSync>> GetGroupsAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        IPaginatedList<GroupSync> result = await GetProfilesAsync<GroupSync>(
            RequestedProfileKind.Group,
            options,
            cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<OrganizationSync>> GetOrganizationsAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        IPaginatedList<OrganizationSync> result = await GetProfilesAsync<OrganizationSync>(
            RequestedProfileKind.Organization,
            options,
            cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<TProfile> GetProfileAsync<TProfile>(string id, CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        var dbKey = $"{_modelsInfo.GetCollectionName<ISyncProfile>()}/{id}";

        Logger.LogDebugMessage("Try to get profile for database key: {key}", dbKey.AsArgumentList());

        var context = CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>();

        GetDocumentResponse<TProfile> result = await base.SendRequestAsync(
            client =>
                client.GetDocumentAsync<TProfile>(dbKey),
            true,
            false,
            context,
            cancellationToken);

        if (result?.Code == HttpStatusCode.NotFound)
        {
            Logger.LogDebugMessage(
                "Attempt to find a profile failed. Could not find a profile with id equals {Id}",
                id.AsArgumentList());

            return Logger.ExitMethod<TProfile>(default);
        }

        Logger.LogDebugMessage(
            "Successfully sent request to arango to get profile with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            result,
            context: context,
            cancellationToken: cancellationToken);

        TProfile profile = result!.Result;

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Successfully got profile with db key {id} and data: {data}",
                LogHelpers.Arguments(dbKey, profile.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Successfully got profile with db key {id} in arango database.",
                dbKey.AsArgumentList());
        }

        return Logger.ExitMethod(profile);
    }

    /// <inheritdoc />
    public async Task<TProfile> CreateProfileAsync<TProfile>(
        TProfile profile,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile
    {
        Logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new ArgumentException("The profile Id should not be null", nameof(profile.Id));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        Logger.LogInfoMessage(
            "Try to create profile with the {id} in arango database.",
            profile.Id.AsArgumentList());

        CreateDocumentResponse response = await CreateEntityInternalAsync(
            profile,
            profile.Id,
            true,
            cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by creating a new group with (Id = {entityId})",
                LogHelpers.Arguments(profile.Id));

            return Logger.ExitMethod(default(TProfile));
        }

        return Logger.ExitMethod(profile);
    }

    /// <inheritdoc />
    public async Task<TProfile> UpdateProfileAsync<TProfile>(
        TProfile profile,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile
    {
        Logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new ArgumentException(
                "The id of the profile can not be null, empty or containing whitespaces.",
                nameof(profile));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<ISyncProfile>()}/{profile.Id}";

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Try to update profile with db key {id} and data in arango database: {data}",
                LogHelpers.Arguments(dbKey, profile.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Try to update profile with db key {id} in arango database.",
                dbKey.AsArgumentList());
        }

        UpdateDocumentResponse<JObject> response = await base.SendRequestAsync(
            client =>
                client.UpdateDocumentAsync(
                    dbKey,
                    profile.InjectDocumentKey(e => e.Id, _serializerSettings)),
            cancellationToken: cancellationToken);

        Logger.LogDebugMessage(
            "Successfully sent request to arango to update profile with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            response,
            context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(profile);
    }

    /// <inheritdoc />
    public async Task DeleteProfileAsync<TProfile>(string id, CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile
    {
        Logger.EnterMethod();

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<ISyncProfile>()}/{id}";

        Logger.LogInfoMessage("Try to delete profile with db key {id} in arango database.", dbKey.AsArgumentList());

        DeleteDocumentResponse response;

        try
        {
            response = await base.SendRequestAsync(
                client =>
                    client.DeleteDocumentAsync(
                        dbKey,
                        new DeleteDocumentOptions
                        {
                            ReturnOld = true
                        }),
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by deleting profile with db key {id} in arango database.",
                dbKey.AsArgumentList());

            throw;
        }

        if (response == null)
        {
            Logger.LogWarnMessage(
                "Error happened by deleting profile with db key {id}, the database return null.",
                dbKey.AsArgumentList());

            Logger.ExitMethod();

            return;
        }

        Logger.LogDebugMessage(
            "Successfully sent request to arango to delete profile with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            response,
            context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<TRole>> GetRolesAsync<TRole>(
        QueryObject options = null,
        CancellationToken cancellationToken = default) where TRole : RoleSync
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting type will be: TRole = {roleName}.",
            LogHelpers.Arguments(typeof(TRole).Name));

        options.Validate(Logger);

        PaginationApiResponse<TRole> response = await ExecuteCountingQueriesAsync<RoleSync, TRole>(
            query => query
                .UsingOptions(options)
                .Compile(CollectionScope.Query),
            cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} roles in total ({responseQueryResultCount} roles in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response.QueryResult
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<RoleSync> CreateRoleAsync(RoleSync role, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (string.IsNullOrWhiteSpace(role.Id))
        {
            throw new ArgumentException("The role Id should not be null or whitespace", nameof(role.Id));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        Logger.LogInfoMessage(
            "Try to create role with the {id} in arango database.",
            role.Id.AsArgumentList());

        CreateDocumentResponse response = await CreateEntityInternalAsync(role, role.Id, true, cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by creating a new group with (Id = {entityId})",
                LogHelpers.Arguments(role.Id));

            return Logger.ExitMethod(default(RoleSync));
        }

        return Logger.ExitMethod(role);
    }

    /// <inheritdoc />
    public async Task<RoleSync> UpdateRoleAsync(RoleSync role, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (string.IsNullOrWhiteSpace(role.Id))
        {
            throw new ArgumentException(
                "The id of the role can not be null, empty or containing whitespaces.",
                nameof(role));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<RoleSync>()}/{role.Id}";

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Try to update role with db key {id} and data in arango database: {data}",
                LogHelpers.Arguments(dbKey, role.ToLogString()));
        }

        Logger.LogInfoMessage(
            "Try to update role with db key {id} in arango database.",
            dbKey.AsArgumentList());

        UpdateDocumentResponse<JObject> response = await base.SendRequestAsync(
            client =>
                client.UpdateDocumentAsync(
                    dbKey,
                    role.InjectDocumentKey(e => e.Id, _serializerSettings)),
            cancellationToken: cancellationToken);

        Logger.LogDebugMessage(
            "Successfully sent request to arango to update role with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            response,
            context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(role);
    }

    /// <inheritdoc />
    public async Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<RoleSync>()}/{roleId}";

        Logger.LogInfoMessage("Try to delete role with db key {id} in arango database.", dbKey.AsArgumentList());

        DeleteDocumentResponse response = await base.SendRequestAsync(
            client =>
                client.DeleteDocumentAsync(
                    dbKey,
                    new DeleteDocumentOptions
                    {
                        ReturnOld = true
                    }),
            cancellationToken: cancellationToken);

        Logger.LogDebugMessage(
            "Successfully sent request to arango to delete role with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            response,
            context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<RoleSync> GetRoleAsync(string id, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("The id should not be null or whitespace", nameof(id));
        }

        var dbKey = $"{_modelsInfo.GetCollectionName<RoleSync>()}/{id}";

        Logger.LogDebugMessage("Try to get profile for database key: {key}", dbKey.AsArgumentList());

        var context = CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>();

        GetDocumentResponse<RoleSync> result = await base.SendRequestAsync(
            client =>
                client.GetDocumentAsync<RoleSync>(dbKey),
            true,
            false,
            context,
            cancellationToken);

        if (result?.Code == HttpStatusCode.NotFound)
        {
            Logger.LogDebugMessage(
                "Attempt to find a profile failed. Could not find a profile with id equals {Id}",
                id.AsArgumentList());

            return Logger.ExitMethod<RoleSync>(default);
        }

        Logger.LogDebugMessage(
            "Successfully sent request to arango to get profile with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            result,
            context: context,
            cancellationToken: cancellationToken);

        RoleSync role = result!.Result;

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Successfully got role with db key {id} and data: {data}",
                LogHelpers.Arguments(dbKey, role.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Successfully got role with db key {id} in arango database.",
                dbKey.AsArgumentList());
        }

        return Logger.ExitMethod(role);
    }

    /// <inheritdoc />
    public async Task<IPaginatedList<TFunction>> GetFunctionsAsync<TFunction>(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default) where TFunction : FunctionSync
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "The resulting types will be: TFunction = {functionName}.",
            LogHelpers.Arguments(typeof(TFunction).Name));

        options.Validate(Logger);

        PaginationApiResponse<TFunction> response =
            await ExecuteCountingQueriesAsync<FunctionSync, TFunction>(
                query => query
                    .UsingOptions(options)
                    .Compile(CollectionScope.Query),
                cancellationToken);

        Logger.LogDebugMessage(
            "Found {responseTotalAmount} functions in total ({responseQueryResultCount} functions in result collection).",
            LogHelpers.Arguments(response.TotalAmount, response.QueryResult.Count));

        return Logger.ExitMethod(
            response.QueryResult
                .ToPaginatedList(response.TotalAmount));
    }

    /// <inheritdoc />
    public async Task<FunctionSync> CreateFunctionAsync(
        FunctionSync function,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (string.IsNullOrWhiteSpace(function.Id))
        {
            throw new ArgumentException("The function Id should not be null or whitespace", nameof(function.Id));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        Logger.LogInfoMessage(
            "Try to create function with the {id} in arango database.",
            function.Id.AsArgumentList());

        CreateDocumentResponse response = await CreateEntityInternalAsync(
            function,
            function.Id,
            true,
            cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by creating a new function with (Id = {entityId})",
                LogHelpers.Arguments(function.Id));

            return Logger.ExitMethod(default(FunctionSync));
        }

        return Logger.ExitMethod(function);
    }

    /// <inheritdoc />
    public async Task<FunctionSync> UpdateFunctionAsync(
        FunctionSync function,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (string.IsNullOrWhiteSpace(function.Id))
        {
            throw new ArgumentException(
                "The id of the function can not be null, empty or containing whitespaces.",
                nameof(function));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<FunctionSync>()}/{function.Id}";

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Try to update function with db key {id} and data in arango database: {data}",
                LogHelpers.Arguments(dbKey, function.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Try to update function with db key {id} in arango database.",
                dbKey.AsArgumentList());
        }

        UpdateDocumentResponse<JObject> response = await base.SendRequestAsync(
            client =>
                client.UpdateDocumentAsync(
                    dbKey,
                    function.InjectDocumentKey(e => e.Id, _serializerSettings)),
            cancellationToken: cancellationToken);

        Logger.LogDebugMessage(
            "Successfully sent request to arango to update function with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            response,
            context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(function);
    }

    /// <inheritdoc />
    public async Task DeleteFunctionAsync(string functionId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<FunctionSync>()}/{functionId}";

        Logger.LogInfoMessage("Try to delete function with db key {id} in arango database.", dbKey.AsArgumentList());

        DeleteDocumentResponse response = await base.SendRequestAsync(
            client =>
                client.DeleteDocumentAsync(
                    dbKey,
                    new DeleteDocumentOptions
                    {
                        ReturnOld = true
                    }),
            cancellationToken: cancellationToken);

        Logger.LogDebugMessage(
            "Successfully sent request to arango to delete function with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            response,
            context: CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>(),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FunctionSync> GetFunctionAsync(string functionId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        var dbKey = $"{_modelsInfo.GetCollectionName<FunctionSync>()}/{functionId}";

        Logger.LogDebugMessage("Try to get function for database key: {key}", dbKey.AsArgumentList());

        var context = CallingServiceContext.CreateNewOf<ArangoSyncEntityStore>();

        GetDocumentResponse<FunctionSync> result = await base.SendRequestAsync(
            client =>
                client.GetDocumentAsync<FunctionSync>(dbKey),
            true,
            false,
            context,
            cancellationToken);

        if (result?.Code == HttpStatusCode.NotFound)
        {
            Logger.LogDebugMessage(
                "Attempt to find a profile failed. Could not find a profile with id equals {Id}",
                functionId.AsArgumentList());

            return Logger.ExitMethod<FunctionSync>(default);
        }

        Logger.LogDebugMessage(
            "Successfully sent request to arango to get role with db key {id}. Response will be checked.",
            dbKey.AsArgumentList());

        await CheckAResponseAsync(
            result,
            context: context,
            cancellationToken: cancellationToken);

        FunctionSync role = result!.Result;

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Successfully got function with db key {id} and data: {data}",
                LogHelpers.Arguments(dbKey, role.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Successfully got function with db key {id} in arango database.",
                dbKey.AsArgumentList());
        }

        return Logger.ExitMethod(role);
    }
}
