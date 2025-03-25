using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.Client.ArangoDb.Public.Models.Transaction;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoFirstLevelProjectionRepository : ArangoRepositoryBase, IFirstLevelProjectionRepository
{
    /// <summary>
    ///     Specifies how many results of <see cref="GetDifferenceInParentsTreesAsync" /> should be loaded per batch.
    ///     The number is decreased as it showed that even simple examples tend to get pretty large.
    /// </summary>
    private const int DifferenceInParentsTreeCursorBatchSize = 20;

    /// <summary>
    ///     Specifies the ArangoDBs query warning for "missing startVertex" which is used in order to detect not existing
    ///     profiles without an extra query.
    /// </summary>
    private const int MissingStartVertexCode = 10;

    /// <summary>
    ///     Holds the information which model should be stored in which database.
    /// </summary>
    private readonly ModelBuilderOptions _modelsInfo;

    /// <summary>
    ///     All operations of <see cref="IProjectionStateRepository" /> will be forwarded to this instance.
    /// </summary>
    private readonly ArangoProjectionStateRepository _projectionStateRepository;

    //TODO change
    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    /// <inheritdoc />
    public ArangoFirstLevelProjectionRepository(
        string arangodbClientName,
        string collectionPrefix,
        ILogger<ArangoFirstLevelProjectionRepository> logger,
        IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        _modelsInfo = DefaultModelConstellation.CreateNewFirstLevelProjection(collectionPrefix).ModelsInfo;
        ArangoDbClientName = arangodbClientName;

        _projectionStateRepository = new ArangoProjectionStateRepository(
            logger,
            serviceProvider,
            arangodbClientName,
            _modelsInfo.GetCollectionName<ProjectionState>());
    }

    private async Task<IList<FirstLevelRelationProfile>> GetAllChildrenInternalAsync(
        string entityId,
        Type entityType,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string entityCollection = _modelsInfo.GetCollectionName(entityType)
            ?? throw new NotSupportedException($"The entity-type {entityType.Name} is not suited.");

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.GetAllChildrenQuery(
            entityCollection,
            entityId,
            _modelsInfo);

        IList<FirstLevelProjectionTraversalResponse<FirstLevelRelationProfile>> response =
            await ExecuteQueryAsync<FirstLevelProjectionTraversalResponse<FirstLevelRelationProfile>>(
                query,
                transaction,
                cancellationToken);

        if (!response.Any() || !response.First().IsStartVertexKnown)
        {
            throw new InstanceNotFoundException("Unable to find the given parent")
            {
                RelatedId = entityId
            };
        }

        return Logger.ExitMethod(response.First().Response);
    }

    private async Task<IList<ObjectIdentPath>> GetAllRelevantObjectsBecauseOfPropertyChangedInternalAsync(
        string entityId,
        Type entityType,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (entityType.IsAssignableFrom(typeof(IFirstLevelProjectionProfile)))
        {
            entityType = typeof(FirstLevelProjectionGroup);
        }

        ParameterizedAql query =
            WellKnownFirstLevelProjectionQueries.GetAllRelevantObjectsBecauseOfPropertyChangedQuery(
                entityId,
                entityType,
                _modelsInfo);

        IList<ObjectIdentPath> result = await ExecuteQueryAsync<ObjectIdentPath>(
            query,
            transaction,
            cancellationToken);

        if (!result.Any())
        {
            throw new InstanceNotFoundException("Unable to find the given entity.")
            {
                RelatedId = entityId
            };
        }

        return Logger.ExitMethod(result);
    }

    private async Task<IList<ObjectIdent>> GetContainerMembersInternal(
        string entityId,
        Type entityType,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string entityCollection = _modelsInfo.GetCollectionName(entityType)
            ?? throw new NotSupportedException($"The entity-type {entityType.Name} is not suited.");

        IList<FirstLevelProjectionTraversalResponse<ObjectIdent>> response =
            await ExecuteQueryAsync<FirstLevelProjectionTraversalResponse<ObjectIdent>>(
                WellKnownFirstLevelProjectionQueries.GetDirectContainerMembers(
                    entityId,
                    entityCollection,
                    _modelsInfo),
                transaction,
                cancellationToken,
                w =>
                {
                    if (w.Code == MissingStartVertexCode)
                    {
                        throw new InstanceNotFoundException("Unable to find the child profile")
                        {
                            RelatedId = entityId
                        };
                    }

                    Logger.LogWarnMessage(
                        "The get function query yielded a warning {message} ({code})",
                        Arguments(w.Message, w.Code));
                });

        if (!response.Any() || !response.First().IsStartVertexKnown)
        {
            throw new InstanceNotFoundException("Unable to find the given member")
            {
                RelatedId = entityId
            };
        }

        List<ObjectIdent> directMembers = response.First().Response;

        return Logger.ExitMethod(directMembers);
    }

    private async Task<IList<IFirstLevelProjectionContainer>> GetParentsInternalAsync(
        string childId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IList<IFirstLevelProjectionContainer> rawParents =
            await ExecuteQueryAsync<IFirstLevelProjectionContainer>(
                WellKnownFirstLevelProjectionQueries.GetParents(_modelsInfo, childId),
                transaction,
                cancellationToken,
                w =>
                {
                    if (w.Code == MissingStartVertexCode)
                    {
                        throw new InstanceNotFoundException("Unable to find the child profile")
                        {
                            RelatedId = childId
                        };
                    }

                    Logger.LogWarnMessage(
                        "The get function query yielded a warning {message} ({code})",
                        Arguments(w.Message, w.Code));
                });

        IList<IFirstLevelProjectionContainer> parents = rawParents.ToList();

        return Logger.ExitMethod(parents);
    }

    private async Task<IList<FirstLevelProjectionsClientSetting>> GetCalculatedClientSettingsInternalAsync(
        string profileId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        IList<FirstLevelProjectionsClientSetting> clientSettings =
            await ExecuteQueryAsync<FirstLevelProjectionsClientSetting>(
                WellKnownFirstLevelProjectionQueries.GetClientSettings(
                    profileId,
                    _modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>(),
                    _modelsInfo.GetCollectionName<FirstLevelProjectionsClientSetting>(),
                    _modelsInfo.GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>()
                        .EdgeCollection,
                    _modelsInfo.GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionClientSettingsBasic>()
                        .EdgeCollection,
                    _modelsInfo.GetCollectionName<FirstLevelProjectionFunction>()),
                transaction,
                cancellationToken,
                w =>
                {
                    if (w.Code == MissingStartVertexCode)
                    {
                        throw new InstanceNotFoundException("Unable to find the requested profile");
                    }

                    Logger.LogWarnMessage(
                        "The query for reading client settings yielded the warning {message}({code})",
                        Arguments(w.Message, w.Code));
                });

        Logger.LogInfoMessage(
            "Found {n} relevant client settings for profile {profileId}",
            Arguments(clientSettings.Count, profileId));

        return Logger.ExitMethod(clientSettings.ToList());
    }

    private IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult>
        GetDifferenceInParentsTreesInternalAsync(
            string profileId,
            IList<string> referenceProfileId,
            ArangoTransaction transaction,
            CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.GetDifferencesInParentTree(
            _modelsInfo,
            profileId,
            referenceProfileId);

        IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> result =
            GetResultWithCursor<FirstLevelProjectionParentsTreeDifferenceResult>(
                transaction,
                query,
                cancellationToken);

        return Logger.ExitMethod(result);
    }

    private async IAsyncEnumerable<T> GetResultWithCursor<T>(
        ArangoTransaction arangoTransaction,
        ParameterizedAql query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        CursorResponse<T> createCursor =
            await arangoTransaction.ExecuteWithLock(
                () => SendRequestAsync(
                    c => c.CreateCursorAsync<T>(
                        new CreateCursorBody
                        {
                            Query = query.Query,
                            BindVars = query.Parameter,
                            BatchSize = DifferenceInParentsTreeCursorBatchSize
                        },
                        arangoTransaction?.TransactionId,
                        cancellationToken: cancellationToken),
                    cancellationToken: cancellationToken),
                cancellationToken);

        if (createCursor == null)
        {
            throw new DataException("The response from ArangoDb does not seem to be correct");
        }

        if (createCursor.Result.Extra.Warnings.Any())
        {
            throw new InstanceNotFoundException("Unable to find the one of the specified profiles");
        }

        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<T> result = createCursor.Result.Result;
        bool hasMore = createCursor.Result.HasMore;

        while (true)
        {
            Logger.LogDebugMessage("Returning new batch of results", Arguments());

            foreach (T value in result)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return value;
            }

            if (!hasMore)
            {
                Logger.LogDebugMessage("Loaded all results from arango", Arguments());

                break;
            }

            cancellationToken.ThrowIfCancellationRequested();

            Logger.LogDebugMessage(
                "Requesting next batch of cursor {cursorId}",
                Arguments(createCursor.CursorDetails.Id));

            PutCursorResponse<T> nextResult = await SendRequestAsync(
                c => c.PutCursorAsync<T>(
                    createCursor.Result.Id,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);

            result = nextResult.Result.Result;
            hasMore = nextResult.Result.HasMore;
        }
    }

    private async Task<FirstLevelProjectionFunction> GetFunctionInternalAsync(
        string functionId,
        ArangoTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        IList<FirstLevelProjectionFunction> functions =
            await ExecuteQueryAsync<FirstLevelProjectionFunction>(
                WellKnownFirstLevelProjectionQueries.GetFunction(_modelsInfo, functionId),
                transaction,
                cancellationToken,
                w =>
                {
                    if (w.Code == MissingStartVertexCode)
                    {
                        throw new InstanceNotFoundException("Unable to find the function")
                        {
                            RelatedId = functionId
                        };
                    }

                    Logger.LogWarnMessage(
                        "The get function query yielded a warning {message} ({code})",
                        Arguments(w.Message, w.Code));
                });

        FirstLevelProjectionFunction function = functions.SingleOrDefault()
            ?? throw new InstanceNotFoundException("Unable to find the function")
            {
                RelatedId = functionId
            };

        return Logger.ExitMethod(function);
    }

    private async Task<TEntity> GetDocumentInternalAsync<TEntity>(
        string entityId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        GetDocumentResponse<TEntity> response = await SendRequestAsync(
            c => c.GetDocumentAsync<TEntity>(
                GetArangoId<TEntity>(entityId),
                transaction?.TransactionId),
            cancellationToken: cancellationToken);

        if (response.Code == HttpStatusCode.NotFound)
        {
            throw new InstanceNotFoundException("Unable to find the entity")
            {
                RelatedId = entityId
            };
        }

        return Logger.ExitMethod(response.Result);
    }

    private async Task DeleteProfileInternalAsync(
        string profileId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        await DeleteEntityInternalAsync<IFirstLevelProjectionProfile>(profileId, transaction, cancellationToken);

        string clientSettingsCollection = _modelsInfo.GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

        IList<string> deletedClientSettings = await ExecuteQueryAsync<string>(
            WellKnownFirstLevelProjectionQueries.DeleteClientSettingsOfUser(clientSettingsCollection, profileId),
            transaction,
            cancellationToken);

        Logger.LogInfoMessage(
            "Deleted {n} client settings for profile {profileId}",
            Arguments(deletedClientSettings.Count, profileId));

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "Deleted client settings {settingsKeys} for profile {profileId}",
                Arguments(JsonConvert.SerializeObject(deletedClientSettings), profileId));
        }

        Logger.ExitMethod();
    }

    private async Task DeleteEntityInternalAsync<TEntity>(
        string entityId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();
        Logger.LogInfoMessage("Deleting {entity} with id {id}", Arguments(typeof(TEntity).Name, entityId));

        // TODO the result does not seem right
        string collection = _modelsInfo.GetCollectionName<TEntity>();

        List<string> edgeCollections = _modelsInfo.GetRelatedOutboundEdgeCollections<TEntity>()
            .Concat(_modelsInfo.GetRelatedInboundEdgeCollections<TEntity>())
            .Distinct()
            .ToList();

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.DeleteEntityWithEdges(
            collection,
            entityId,
            edgeCollections);

        IList<string> deleted = await ExecuteQueryAsync<string>(
            query,
            transaction,
            cancellationToken);

        if (!deleted.Any())
        {
            throw new InstanceNotFoundException
            {
                RelatedId = entityId
            };
        }

        await RemoveTemporaryAssignmentInternalAsync(
            entityId,
            transaction,
            cancellationToken);

        Logger.ExitMethod();
    }

    private async Task<IList<FirstLevelProjectionTemporaryAssignment>> GetTemporaryAssignmentsInternalAsync(
        ArangoTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        IList<FirstLevelProjectionTemporaryAssignment> assignments =
            await ExecuteQueryAsync<FirstLevelProjectionTemporaryAssignment>(
                WellKnownFirstLevelProjectionQueries.GetTemporaryAssignments(
                    _modelsInfo.GetCollectionName<FirstLevelProjectionTemporaryAssignment>()),
                transaction,
                cancellationToken);

        return Logger.ExitMethod(assignments);
    }

    private async Task UpdateTemporaryAssignmentStatesInternalAsync(
        IList<FirstLevelProjectionTemporaryAssignment> desiredStates,
        ArangoTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ParameterizedAql aql = WellKnownFirstLevelProjectionQueries.ReplaceExistingTemporaryAssignments(
            desiredStates,
            _modelsInfo.GetCollectionName<FirstLevelProjectionTemporaryAssignment>());

        IList<string> modifiedIds =
            await ExecuteQueryAsync<string>(
                aql,
                transaction,
                cancellationToken);

        int missingItems = desiredStates
            .Select(s => s.Id)
            .Except(modifiedIds, StringComparer.OrdinalIgnoreCase)
            .Count();

        if (missingItems > 0)
        {
            throw new StatesMismatchException(
                "Error during saving temporary assignments. Mismatch of state to be saved and already persisted state.");
        }

        Logger.LogDebugMessage("State saved.", LogHelpers.Arguments());

        Logger.ExitMethod();
    }

    private async Task SetUpdatedAtInternalAsync(
        DateTime updatedAt,
        IList<ObjectIdent> objects,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IEnumerable<IGrouping<Type, string>> groups = objects
            .Select(
                o => (
                    Type: o.Type.ToFirstLevelEntityType(false),
                    o.Id))
            .GroupBy(o => o.Type, o => o.Id);

        foreach (IGrouping<Type, string> objectGroup in groups)
        {
            Logger.LogInfoMessage(
                "Updating {n} updatedAts of {type}",
                Arguments(objectGroup.Count(), objectGroup.Key.Name));

            await SendRequestAsync(
                c => c.UpdateDocumentsAsync(
                    _modelsInfo.GetCollectionName(objectGroup.Key),
                    objectGroup.Select(
                            id => JObject.FromObject(
                                new Dictionary<string, object>
                                {
                                    { nameof(IFirstLevelProjectionProfile.UpdatedAt), updatedAt },
                                    { AConstants.KeySystemProperty, id }
                                },
                                JsonSerializer.Create(c.UsedJsonSerializerSettings)))
                        .ToArray(),
                    new UpdateDocumentOptions
                    {
                        MergeObjects = true
                    },
                    transaction?.TransactionId),
                cancellationToken: cancellationToken);
        }

        Logger.ExitMethod();
    }

    private async Task ReplaceDocumentInternalAsync<TEntity>(
        string entityId,
        TEntity entity,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string collectionName = _modelsInfo.GetCollectionName<TEntity>();

        ReplaceDocumentResponse response = await SendRequestAsync(
            c => c.ReplaceDocumentAsync(
                $"{collectionName}/{entityId}",
                entity,
                transactionId: transaction?.TransactionId),
            cancellationToken: cancellationToken);

        if (response.Code == HttpStatusCode.NotFound)
        {
            throw new InstanceNotFoundException($"Unable to find the {typeof(TEntity)} with the specified id.")
            {
                RelatedId = entityId
            };
        }

        Logger.ExitMethod();
    }

    private async Task CreateProfileInternalAsync(
        IFirstLevelProjectionProfile profile,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string profilesCollection = _modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>();

        Logger.LogDebugMessage(
            "Creating profile {profileId} in collection {collectionName}",
            LogHelpers.Arguments(profile.Id, profilesCollection));

        await SendRequestAsync(
            c => c.CreateDocumentAsync(
                profilesCollection,
                profile.InjectDocumentKey(p => p.Id, c.UsedJsonSerializerSettings),
                new CreateDocumentOptions
                {
                    OverWriteMode = AOverwriteMode.Conflict
                },
                transaction?.TransactionId),
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    private async Task CreateRoleInternalAsync(
        FirstLevelProjectionRole role,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string rolesCollection = _modelsInfo.GetCollectionName<FirstLevelProjectionRole>();

        Logger.LogDebugMessage(
            "Creating role {roleId} in collection {collectionName}",
            LogHelpers.Arguments(role.Id, rolesCollection));

        await SendRequestAsync(
            c => c.CreateDocumentAsync(
                rolesCollection,
                role.InjectDocumentKey(r => r.Id, c.UsedJsonSerializerSettings),
                new CreateDocumentOptions
                {
                    OverWriteMode = AOverwriteMode.Conflict
                },
                transaction?.TransactionId),
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    private async Task CreateFunctionInternalAsync(
        FirstLevelProjectionFunction function,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IArangoDbClient client = GetArangoDbClient();
        string functionsCollection = _modelsInfo.GetCollectionName<FirstLevelProjectionFunction>();
        cancellationToken.ThrowIfCancellationRequested();

        CreateDocumentResponse createFunctionResult = await client.CreateDocumentAsync(
            functionsCollection,
            function.InjectDocumentKey(f => f.Id),
            new CreateDocumentOptions
            {
                OverWriteMode = AOverwriteMode.Conflict
            },
            transaction?.TransactionId);

        await CheckAResponseAsync(
            createFunctionResult,
            context: transaction?.CallingService,
            cancellationToken: cancellationToken);

        await InsertEdgeAsync(
            function.Id,
            typeof(FirstLevelProjectionFunction),
            function.Role.Id,
            typeof(FirstLevelProjectionRole),
            transaction,
            cancellationToken: cancellationToken);

        await InsertEdgeAsync(
            function.Id,
            typeof(FirstLevelProjectionFunction),
            function.Organization.Id,
            typeof(FirstLevelProjectionOrganization),
            transaction,
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    private async Task CreateTagInternalAsync(
        FirstLevelProjectionTag tag,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string tagsCollection = _modelsInfo.GetCollectionName<FirstLevelProjectionTag>();

        Logger.LogDebugMessage(
            "Creating tag {tagId} in collection {collectionName}",
            LogHelpers.Arguments(tag.Id, tagsCollection));

        await SendRequestAsync(
            c => c.CreateDocumentAsync(
                tagsCollection,
                tag.InjectDocumentKey(t => t.Id, c.UsedJsonSerializerSettings),
                new CreateDocumentOptions
                {
                    OverWriteMode = AOverwriteMode.Conflict
                },
                transaction?.TransactionId),
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    private async Task CreateProfileAssignmentInternalAsync(
        string parentId,
        ContainerType parentType,
        string profileId,
        IList<RangeCondition> conditions,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.CreateAssignment(
            profileId,
            parentId,
            parentType.ToFirstLevelEntityType(),
            conditions.ToArray(),
            _modelsInfo);

        IList<FirstLevelProjectionAssignment> result =
            await ExecuteQueryAsync<FirstLevelProjectionAssignment>(query, transaction, cancellationToken);

        if (!result.Any())
        {
            throw new InstanceNotFoundException("Unable to find at least one of the entities");
        }

        await CreateTemporaryAssignmentEntriesInternalAsync(
            parentId,
            parentType,
            profileId,
            conditions,
            transaction,
            cancellationToken);

        Logger.ExitMethod();
    }

    private async Task CreateTemporaryAssignmentEntriesInternalAsync(
        string parentId,
        ContainerType parentType,
        string profileId,
        IList<RangeCondition> conditions,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IList<FirstLevelProjectionTemporaryAssignment> temporaryAssignments = conditions
            .ToTemporaryAssignments(
                profileId,
                parentId,
                parentType);

        if (temporaryAssignments.Count == 0)
        {
            Logger.LogTraceMessage("No temporary assignments found - skipping method", LogHelpers.Arguments());
            Logger.ExitMethod();

            return;
        }

        // This is necessary call, because the object type is not known.
        // It could be done inside the modification AQL that contains insert commands,
        // but the profileKind-to-objectType conversion must be done in AQL
        // As for now that would be rather easy, because the string names of both enums are equal.
        // However, this can be changed in future
        // and if so, the compiler won't report errors if a conversion method is not valid or present
        var profile = await GetDocumentInternalAsync<IFirstLevelProjectionProfile>(
            profileId,
            transaction,
            cancellationToken);

        ParameterizedAql aql = WellKnownFirstLevelProjectionQueries.InsertTemporaryAssignmentEntries(
            profile.Kind.ToObjectType(),
            temporaryAssignments,
            _modelsInfo.GetCollectionName<FirstLevelProjectionTemporaryAssignment>());

        if (Logger.IsEnabledForTrace())
        {
            string debugInfo = JsonConvert.SerializeObject(aql.Parameter);

            Logger.LogTraceMessage(
                "Used AQL to insert temporary assignments: {usedAql}",
                LogHelpers.Arguments(aql.Query));

            Logger.LogTraceMessage(
                "Used parameters inside AQL: {aqlParameters}",
                LogHelpers.Arguments(debugInfo));
        }

        IList<FirstLevelProjectionTemporaryAssignment> created =
            await ExecuteQueryAsync<FirstLevelProjectionTemporaryAssignment>(
                aql,
                transaction,
                cancellationToken);

        // if some entries were written, but not all of expected, giving a warning would be nice
        if (temporaryAssignments.Count != created.Count)
        {
            Logger.LogWarnMessage(
                "Some entries of new temporary assignments were not created. Expected amount: {expectedAmount}; done: {currentAmount}",
                LogHelpers.Arguments(temporaryAssignments.Count, created.Count));
        }

        Logger.LogInfoMessage("Entries of new temporary assignments created", LogHelpers.Arguments());

        Logger.ExitMethod();
    }

    private Task AddTagAsync<TEntity>(
        FirstLevelProjectionTagAssignment tag,
        string entityId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Task task = InsertEdgeAsync(
            entityId,
            typeof(TEntity),
            tag.TagId,
            typeof(FirstLevelProjectionTag),
            transaction,
            new Dictionary<string, object>
            {
                { nameof(FirstLevelProjectionTagAssignment.IsInheritable), tag.IsInheritable }
            },
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    private async Task DeleteProfileAssignmentInternalAsync(
        string parentId,
        Type parentType,
        string profileId,
        IList<RangeCondition> conditions,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.UpdateAssignments(
            profileId,
            parentId,
            parentType,
            conditions.ToArray(),
            _modelsInfo);

        Logger.LogDebugMessage("Updating assignments", Arguments());

        IList<FirstLevelProjectionAssignment> result =
            await ExecuteQueryAsync<FirstLevelProjectionAssignment>(query, transaction, cancellationToken);

        if (!result.Any())
        {
            throw new InstanceNotFoundException("Unable to find the given assignment");
        }

        await RemoveTemporaryAssignmentInternalAsync(
            profileId,
            parentId,
            conditions,
            transaction,
            cancellationToken);

        string assignmentsCollection = _modelsInfo.GetRelation(typeof(IFirstLevelProjectionProfile), parentType)
            ?.EdgeCollection;

        List<string> toDelete = result.Where(r => !r.Conditions.Any()).Select(r => r.ArangoKey).ToList();

        if (!toDelete.Any())
        {
            Logger.LogInfoMessage(
                "Updated assignment edge. There are some other assignments deleting the edge from being deleted",
                Arguments());

            Logger.ExitMethod();

            return;
        }

        Logger.LogInfoMessage(
            "Deleting {n} assignment edges as there are no conditions left",
            Arguments(toDelete.Count));

        await SendRequestAsync(
            c => c.DeleteDocumentsAsync(assignmentsCollection, toDelete, transactionId: transaction?.TransactionId),
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    private Task RemoveTemporaryAssignmentInternalAsync(
        string relatedObjectId, // either profile or target
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        return RemoveTemporaryAssignmentInternalAsync(relatedObjectId, null, null, transaction, cancellationToken);
    }

    private async Task RemoveTemporaryAssignmentInternalAsync(
        string profileId,
        string targetId,
        IList<RangeCondition> conditions,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();

        string collectionName = _modelsInfo.GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

        ParameterizedAql aql =
            targetId != null
                ? WellKnownFirstLevelProjectionQueries.DeleteSpecifiedTemporaryAssignments(
                    profileId,
                    targetId,
                    conditions,
                    collectionName)
                : WellKnownFirstLevelProjectionQueries.DeleteAllTemporaryAssignmentsRelatedToObject(
                    profileId,
                    collectionName);

        if (Logger.IsEnabledForTrace())
        {
            string debugInfo = JsonConvert.SerializeObject(aql.Parameter);

            Logger.LogTraceMessage(
                "Used AQL to insert temporary assignments: {usedAql}",
                LogHelpers.Arguments(aql.Query));

            Logger.LogTraceMessage(
                "Used parameters inside AQL: {aqlParameters}",
                LogHelpers.Arguments(debugInfo));
        }

        IList<string> deletedIds = await ExecuteQueryAsync<string>(
            aql,
            transaction,
            cancellationToken);

        if (Logger.IsEnabledFor(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "Entries in collection of temporary assignments deleted: {deleted}",
                deletedIds.ToLogString().AsArgumentList());
        }
        else
        {
            Logger.LogInfoMessage(
                "Amount of entries in collection of temporary assignments deleted: {deletedAmount}",
                deletedIds.Count.AsArgumentList());
        }

        Logger.ExitMethod();
    }

    private async Task RemoveTagInternalAsync<TEntity>(
        string tagId,
        string entityId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        string arangoEntityId = GetArangoId<TEntity>(entityId);
        string arangoTagId = GetArangoId<FirstLevelProjectionTag>(tagId);

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.DeleteTagLink(
            _modelsInfo.GetRelation<TEntity, FirstLevelProjectionTag>().EdgeCollection
            ?? throw new NotSupportedException("The ModelBuilder does not seem to be setup correctly"),
            arangoTagId,
            arangoEntityId);

        IList<FirstLevelProjectionTagAssignment> deleted =
            await ExecuteQueryAsync<FirstLevelProjectionTagAssignment>(query, transaction, cancellationToken);

        if (!deleted.Any())
        {
            throw new InstanceNotFoundException("Unable to delete the tag link as is could not be found");
        }
    }

    private async Task SetClientSettingsInternalAsync(
        string profileId,
        string clientSetting,
        string key,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string clientSettingsCollection = _modelsInfo.GetCollectionName<FirstLevelProjectionsClientSetting>();
        IArangoDbClient client = GetArangoDbClient();

        IList<string> clientSettings = await ExecuteQueryAsync<string>(
            WellKnownFirstLevelProjectionQueries.UpsertDocument(
                clientSettingsCollection,
                JObject.FromObject(
                    new Dictionary<string, object>
                    {
                        {
                            nameof(FirstLevelProjectionClientSettingsBasic
                                .ProfileId),
                            profileId
                        },
                        { nameof(FirstLevelProjectionClientSettingsBasic.Key), key }
                    },
                    JsonSerializer.Create(client.UsedJsonSerializerSettings)),
                JObject.FromObject(
                    new FirstLevelProjectionClientSettingsBasic
                    {
                        Key = key,
                        ProfileId = profileId,
                        Value = clientSetting
                    },
                    JsonSerializer.Create(client.UsedJsonSerializerSettings))),
            transaction,
            cancellationToken);

        if (clientSettings.Any(c => c == null))
        {
            throw new DatabaseException(
                $"An error occurred while saving the client settings for profile {profileId}",
                ExceptionSeverity.Error);
        }

        string documentId = clientSettings.First();

        try
        {
            await InsertEdgeAsync(
                profileId,
                typeof(IFirstLevelProjectionProfile),
                documentId,
                typeof(FirstLevelProjectionClientSettingsBasic),
                transaction,
                cancellationToken: cancellationToken);
        }
        catch (InstanceNotFoundException)
        {
            Logger.LogInfoMessage(
                "Deleting created client settings document {key} for {profileId}",
                Arguments(documentId, profileId));

            DeleteDocumentResponse response = await SendRequestAsync(
                c => c.DeleteDocumentAsync(
                    clientSettingsCollection,
                    documentId,
                    transactionId: transaction?.TransactionId),
                // we can ignore exceptions because it is only a cleanup. No need to throw the actual exception as the cause still remains the instance not found.
                false,
                cancellationToken: cancellationToken);

            if (response.Error)
            {
                Logger.LogWarnMessage(
                    response.Exception,
                    "Unable to delete the newly created client settings document {key} for {profileId}",
                    Arguments(documentId, profileId));
            }

            throw;
        }

        Logger.ExitMethod();
    }

    private async Task UnsetClientSettingsInternalAsync(
        string profileId,
        string key,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string clientSettingsCollection = _modelsInfo.GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

        IList<string> documents = await ExecuteQueryAsync<string>(
            WellKnownFirstLevelProjectionQueries.FindClientSettings(clientSettingsCollection, profileId, key),
            transaction,
            cancellationToken);

        if (!documents.Any())
        {
            throw new InstanceNotFoundException($"Unable to find the client setting '{key}' for the profile")
            {
                RelatedId = profileId
            };
        }

        await DeleteEntityInternalAsync<FirstLevelProjectionsClientSetting>(
            documents.Single(),
            transaction,
            cancellationToken);

        Logger.ExitMethod();
    }

    /// <summary>
    ///     Validates the given <see cref="IDatabaseTransaction" /> and throws an exception if the exception is not valid.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <returns>The <see cref="ArangoTransaction" /> which was passed.</returns>
    /// <exception cref="ArgumentException">Will be yielded if the transaction is not suitable for this repository.</exception>
    private ArangoTransaction ValidateTransaction(IDatabaseTransaction transaction)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            return Logger.ExitMethod<ArangoTransaction>(null);
        }

        if (transaction is not ArangoTransaction arangoTransaction)
        {
            throw new ArgumentException(
                "The passed transaction is not suited for ArangoDB operations.",
                nameof(transaction));
        }

        if (!arangoTransaction.IsActive)
        {
            throw new ArgumentException(
                "The given transaction is not active anymore and has either been committed or aborted.",
                nameof(transaction));
        }

        if (string.IsNullOrWhiteSpace(arangoTransaction.TransactionId))
        {
            throw new ArgumentException(
                "The passed transaction is not suited for ArangoDB operations. The transactionId must not be null or only contain whitespaces.",
                nameof(transaction));
        }

        if (!_modelsInfo.GetDocumentCollections()
                .Concat(_modelsInfo.GetEdgeCollections())
                .All(arangoTransaction.Collections.Contains))
        {
            throw new ArgumentException(
                "The passed transaction is not suited for ArangoDB operations. All first level projection collections have to be included within the transaction.",
                nameof(transaction));
        }

        return Logger.ExitMethod(arangoTransaction);
    }

    internal async Task<IList<TResponse>> ExecuteQueryAsync<TResponse>(
        ParameterizedAql query,
        ArangoTransaction transaction,
        CancellationToken cancellationToken,
        Action<CursorResponseWarning> handleWarnings = null)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Sending query {query} to ArangoDB with bind-vars {bindVars}",
                LogHelpers.Arguments(query.Query, JsonConvert.SerializeObject(query.Parameter)));
        }

        MultiApiResponse<TResponse> result = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<TResponse>(
                new CreateCursorBody
                {
                    BindVars = query.Parameter,
                    Query = query.Query
                },
                transaction?.TransactionId,
                cancellationToken: cancellationToken),
            cancellationToken: cancellationToken);

        if (handleWarnings != null)
        {
            List<CursorResponseWarning> warnings = result.Responses.Where(r => r is CursorResponse<TResponse>)
                .Cast<CursorResponse<TResponse>>()
                .SelectMany(r => r.Result.Extra.Warnings)
                .ToList();

            Logger.LogDebugMessage("Query executed with {n} warnings", Arguments(warnings.Count));
            warnings.ForEach(handleWarnings);
        }

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "The query yielded {results} results",
                LogHelpers.Arguments(result.QueryResult.Count));
        }

        return Logger.ExitMethod(result.QueryResult.ToList());
    }

    protected async Task CommitTransactionInternalAsync(
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IArangoDbClient client = GetArangoDbClient();
        TransactionOperationResponse response = await client.CommitTransactionAsync(transaction.TransactionId);
        transaction.MarkAsInactive();

        cancellationToken.ThrowIfCancellationRequested();

        await CheckAResponseAsync(
            response,
            true,
            context: transaction.CallingService,
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    protected async Task AbortTransactionInternalAsync(
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IArangoDbClient client = GetArangoDbClient();
        TransactionOperationResponse response = await client.AbortTransactionAsync(transaction.TransactionId);
        transaction.MarkAsInactive();

        cancellationToken.ThrowIfCancellationRequested();

        await CheckAResponseAsync(
            response,
            true,
            context: transaction.CallingService,
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    protected string GetArangoId<TEntity>(string entityId)
    {
        return $"{_modelsInfo.GetCollectionName<TEntity>()}/{entityId}";
    }

    protected async Task InsertEdgeAsync(
        string fromId,
        Type fromType,
        string toId,
        Type toType,
        ArangoTransaction transaction,
        Dictionary<string, object> extraProperties = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        string fromCollection = _modelsInfo.GetCollectionName(fromType);
        string toCollection = _modelsInfo.GetCollectionName(toType);

        ModelBuilderOptionsTypeRelation relation = _modelsInfo.GetRelation(fromType, toType)
            ?? throw new NotSupportedException($"Unable to assign {fromType.Name} to {toType.Name}");

        ParameterizedAql query = WellKnownFirstLevelProjectionQueries.InsertEdge(
            relation.EdgeCollection,
            fromCollection,
            fromId,
            toCollection,
            toId,
            relation.FromProperties,
            relation.ToProperties,
            JObject.FromObject(extraProperties ?? new Dictionary<string, object>()));

        IList<string> createdEdges = await ExecuteQueryAsync<string>(
            query,
            transaction,
            cancellationToken);

        if (!createdEdges.Any())
        {
            throw new InstanceNotFoundException();
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<IDatabaseTransaction> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        IArangoDbClient client = GetArangoDbClient();

        IList<string> collections = _modelsInfo.GetDocumentCollections()
            .Concat(_modelsInfo.GetEdgeCollections())
            .ToImmutableList();

        TransactionOperationResponse transactionResponse =
            await client.BeginTransactionAsync(collections, Array.Empty<string>());

        await CheckAResponseAsync(
            transactionResponse,
            true,
            context: CallingServiceContext.CreateNewOf<ArangoFirstLevelProjectionRepository>(),
            cancellationToken: cancellationToken);

        IDatabaseTransaction transaction = new ArangoTransaction
        {
            Collections = collections,
            TransactionId = transactionResponse.GetTransactionId()
        };

        return Logger.ExitMethod(transaction);
    }

    /// <inheritdoc />
    public Task CommitTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CommitTransactionInternalAsync(arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => AbortTransactionInternalAsync(arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<IList<FirstLevelRelationProfile>> GetAllChildrenAsync(
        ObjectIdent parentId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (parentId == null)
        {
            throw new ArgumentNullException(nameof(parentId));
        }

        if (string.IsNullOrWhiteSpace(parentId.Id))
        {
            throw new ArgumentException(nameof(parentId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IList<FirstLevelRelationProfile>> task = arangoTransaction.ExecuteWithLock(
            () => GetAllChildrenInternalAsync(
                parentId.Id,
                parentId.Type.ToFirstLevelEntityType(),
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<IList<ObjectIdentPath>> GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
        ObjectIdent idOfModifiedObject,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (idOfModifiedObject == null)
        {
            throw new ArgumentNullException(nameof(idOfModifiedObject));
        }

        if (string.IsNullOrWhiteSpace(idOfModifiedObject.Id))
        {
            throw new ArgumentException(nameof(idOfModifiedObject));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IList<ObjectIdentPath>> task = arangoTransaction.ExecuteWithLock(
            () => GetAllRelevantObjectsBecauseOfPropertyChangedInternalAsync(
                idOfModifiedObject.Id,
                idOfModifiedObject.Type.ToFirstLevelEntityType(),
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public async Task<IList<FirstLevelProjectionTagAssignment>> GetTagsAssignmentsFromProfileAsync(
        string[] tagIds,
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagIds == null)
        {
            throw new ArgumentNullException(nameof(tagIds));
        }

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (tagIds.Length == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(tagIds));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        IList<FirstLevelProjectionTagAssignment> task = await arangoTransaction.ExecuteWithLock(
            () => ExecuteQueryAsync<FirstLevelProjectionTagAssignment>(
                WellKnownFirstLevelProjectionQueries.GetTagLinks(
                    GetArangoId<IFirstLevelProjectionProfile>(profileId),
                    _modelsInfo.GetRelation<IFirstLevelProjectionProfile, FirstLevelProjectionTag>()
                        .EdgeCollection,
                    tagIds),
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<IList<ObjectIdent>> GetContainerMembersAsync(
        ObjectIdent container,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        if (container.Id == null)
        {
            throw new ArgumentNullException(nameof(container.Id));
        }

        if (string.IsNullOrWhiteSpace(container.Id))
        {
            throw new ArgumentException("Value cannot not be null or whitespace.", nameof(container.Id));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IList<ObjectIdent>> directMembers = arangoTransaction.ExecuteWithLock(
            () => GetContainerMembersInternal(
                container.Id,
                container.Type.ToFirstLevelEntityType(),
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(directMembers);
    }

    /// <inheritdoc />
    public Task<IList<IFirstLevelProjectionContainer>> GetParentsAsync(
        string childId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (childId == null)
        {
            throw new ArgumentNullException(nameof(childId));
        }

        if (string.IsNullOrWhiteSpace(childId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(childId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IList<IFirstLevelProjectionContainer>> task = arangoTransaction.ExecuteWithLock(
            () => GetParentsInternalAsync(childId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<IList<ObjectIdent>> GetAssignedObjectsFromTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagId == null)
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        if (string.IsNullOrWhiteSpace(tagId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tagId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IList<ObjectIdent>> task = arangoTransaction.ExecuteWithLock(
            () => ExecuteQueryAsync<ObjectIdent>(
                WellKnownFirstLevelProjectionQueries.GetLinkedObjectsToTags(tagId, _modelsInfo),
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<IFirstLevelProjectionProfile> GetProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IFirstLevelProjectionProfile> task =
            arangoTransaction.ExecuteWithLock(
                () => GetDocumentInternalAsync<IFirstLevelProjectionProfile>(
                    profileId,
                    arangoTransaction,
                    cancellationToken),
                cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<TProfileType> GetProfileAsync<TProfileType>(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default) where TProfileType : class, IFirstLevelProjectionProfile
    {
        Logger.EnterMethod();
        var tcs = new TaskCompletionSource<TProfileType>();

        return Logger.ExitMethod(
            GetProfileAsync(profileId, transaction, cancellationToken)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else if (t.IsFaulted)
                        {
                            tcs.TrySetException(t.Exception!);
                        }
                        else
                        {
                            if (t.Result is TProfileType type)
                            {
                                tcs.TrySetResult(type);
                            }
                            else
                            {
                                // since its not the expected type instance not found will be thrown
                                throw new InstanceNotFoundException(
                                    null,
                                    profileId,
                                    $"No {typeof(TProfileType).Name} found.");
                            }
                        }

                        return tcs.Task;
                    },
                    TaskContinuationOptions.ExecuteSynchronously)
                .Unwrap());
    }

    /// <inheritdoc />
    public Task<IList<FirstLevelProjectionsClientSetting>> GetCalculatedClientSettingsAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<IList<FirstLevelProjectionsClientSetting>> task =
            arangoTransaction.ExecuteWithLock(
                () => GetCalculatedClientSettingsInternalAsync(profileId, arangoTransaction, cancellationToken),
                cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> GetDifferenceInParentsTreesAsync(
        string profileId,
        IList<string> referenceProfileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (referenceProfileId == null)
        {
            throw new ArgumentNullException(nameof(referenceProfileId));
        }

        if (referenceProfileId.Count == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(referenceProfileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> task =
            GetDifferenceInParentsTreesInternalAsync(
                profileId,
                referenceProfileId,
                arangoTransaction,
                cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<FirstLevelProjectionRole> GetRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (roleId == null)
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(roleId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<FirstLevelProjectionRole> task =
            arangoTransaction.ExecuteWithLock(
                () => GetDocumentInternalAsync<FirstLevelProjectionRole>(
                    roleId,
                    arangoTransaction,
                    cancellationToken),
                cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public async Task<ICollection<FirstLevelProjectionFunction>> GetFunctionsOfOrganizationAsync(
        string organizationId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (organizationId == null)
        {
            throw new ArgumentNullException(nameof(organizationId));
        }

        if (string.IsNullOrWhiteSpace(organizationId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(organizationId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        ParameterizedAql query =
            WellKnownFirstLevelProjectionQueries.GetFunctionsOfOrganization(_modelsInfo, organizationId);

        IList<FirstLevelProjectionFunction> result =
            await ExecuteQueryAsync<FirstLevelProjectionFunction>(
                query,
                arangoTransaction,
                cancellationToken,
                w =>
                {
                    if (w.Code == MissingStartVertexCode)
                    {
                        throw new InstanceNotFoundException("Unable to get functions of organization.");
                    }

                    Logger.LogWarnMessage(
                        "The query for getting function of organization yielded the warning {message}({code})",
                        Arguments(w.Message, w.Code));
                });

        return result;
    }

    /// <inheritdoc />
    public Task<FirstLevelProjectionFunction> GetFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (functionId == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(functionId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<FirstLevelProjectionFunction> task = arangoTransaction.ExecuteWithLock(
            () => GetFunctionInternalAsync(functionId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<FirstLevelProjectionTag> GetTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagId == null)
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        if (string.IsNullOrWhiteSpace(tagId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tagId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<FirstLevelProjectionTag> task =
            arangoTransaction.ExecuteWithLock(
                () => GetDocumentInternalAsync<FirstLevelProjectionTag>(
                    tagId,
                    arangoTransaction,
                    cancellationToken),
                cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task DeleteProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => DeleteProfileInternalAsync(profileId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task DeleteRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (roleId == null)
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(roleId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => DeleteEntityInternalAsync<FirstLevelProjectionRole>(
                roleId,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task DeleteFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (functionId == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(functionId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => DeleteEntityInternalAsync<FirstLevelProjectionFunction>(
                functionId,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task DeleteTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagId == null)
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        if (string.IsNullOrWhiteSpace(tagId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tagId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => DeleteEntityInternalAsync<FirstLevelProjectionTag>(tagId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task UpdateProfileAsync(
        IFirstLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => ReplaceDocumentInternalAsync(profile.Id, profile, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task UpdateRoleAsync(
        FirstLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => ReplaceDocumentInternalAsync(role.Id, role, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task SetUpdatedAtAsync(
        DateTime updatedAt,
        IList<ObjectIdent> objects,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (objects == null)
        {
            throw new ArgumentNullException(nameof(objects));
        }

        if (objects.Count == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(objects));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => SetUpdatedAtInternalAsync(
                updatedAt.ToUniversalTime(),
                objects,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public async Task<IList<FirstLevelProjectionTemporaryAssignment>> GetTemporaryAssignmentsAsync(
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        IList<FirstLevelProjectionTemporaryAssignment> assignments =
            await arangoTransaction.ExecuteWithLock(
                () => GetTemporaryAssignmentsInternalAsync(arangoTransaction, cancellationToken),
                cancellationToken);

        Logger.LogInfoMessage(
            "Found {count} temporary assignment entries in total.",
            Arguments(assignments?.Count ?? 0));

        return Logger.ExitMethod(assignments);
    }

    /// <inheritdoc />
    public async Task UpdateTemporaryAssignmentStatesAsync(
        IList<FirstLevelProjectionTemporaryAssignment> desiredStates,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (desiredStates == null)
        {
            throw new ArgumentNullException(nameof(desiredStates));
        }

        if (desiredStates.Count == 0)
        {
            Logger.LogDebugMessage("Did not get any states to save - skipping method", LogHelpers.Arguments());
            Logger.ExitMethod();

            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        await arangoTransaction.ExecuteWithLock(
            () => UpdateTemporaryAssignmentStatesInternalAsync(
                desiredStates,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public Task UpdateFunctionAsync(
        FirstLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => ReplaceDocumentInternalAsync(function.Id, function, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task CreateProfileAsync(
        IFirstLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CreateProfileInternalAsync(profile, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task CreateRoleAsync(
        FirstLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CreateRoleInternalAsync(role, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task CreateFunctionAsync(
        FirstLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CreateFunctionInternalAsync(function, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task CreateTag(
        FirstLevelProjectionTag tag,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CreateTagInternalAsync(tag, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task CreateProfileAssignmentAsync(
        string parentId,
        ContainerType parentType,
        string profileId,
        IList<RangeCondition> conditions,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (parentId == null)
        {
            throw new ArgumentNullException(nameof(parentId));
        }

        if (conditions == null)
        {
            throw new ArgumentNullException(nameof(conditions));
        }

        if (string.IsNullOrWhiteSpace(parentId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentId));
        }

        if (!Enum.IsDefined(typeof(ContainerType), parentType))
        {
            throw new InvalidEnumArgumentException(nameof(parentType), (int)parentType, typeof(ContainerType));
        }

        if (parentType == ContainerType.NotSpecified)
        {
            throw new ArgumentException(
                "NotSpecified is not allowed when creating an assignment",
                nameof(parentType));
        }

        if (conditions.Count == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(conditions));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CreateProfileAssignmentInternalAsync(
                parentId,
                parentType,
                profileId,
                conditions,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task AddTagToProfileAsync(
        FirstLevelProjectionTagAssignment tag,
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => AddTagAsync<IFirstLevelProjectionProfile>(tag, profileId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task AddTagToRoleAsync(
        FirstLevelProjectionTagAssignment tag,
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        if (roleId == null)
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(roleId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => AddTagAsync<FirstLevelProjectionRole>(tag, roleId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task AddTagToFunctionAsync(
        FirstLevelProjectionTagAssignment tag,
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        if (functionId == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(functionId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => AddTagAsync<FirstLevelProjectionFunction>(
                tag,
                functionId,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public async Task<bool> UserExistAsync(
        string externalId,
        string displayName,
        string email,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(externalId)
            && string.IsNullOrWhiteSpace(displayName)
            && string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("External Id, displayName and email should not be all null or whitespace");
        }

        string collectionName = _modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>();
        ParameterizedAql aqlQuery = WellKnownFirstLevelProjectionQueries.UserExist(
            collectionName,
            externalId,
            email,
            displayName);

        IList<bool> result = await ExecuteQueryAsync<bool>(aqlQuery, null, cancellationToken);

        return Logger.ExitMethod(result.FirstOrDefault());
    }

    /// <inheritdoc />
    public async Task<bool> GroupExistAsync(
        string externalId,
        string name,
        string displayName,
        bool ignoreCase,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(externalId)
            && string.IsNullOrWhiteSpace(displayName)
            && string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("External Id, displayName and name of the group should not be all null or whitespace");
        }

        string collectionName = _modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>();
        ParameterizedAql aqlQuery = WellKnownFirstLevelProjectionQueries.GroupExist(
            collectionName,
            externalId,
            name,
            displayName, ignoreCase);

        IList<bool> result = await ExecuteQueryAsync<bool>(aqlQuery, null, cancellationToken);

        return Logger.ExitMethod(result.FirstOrDefault());
    }

    /// <inheritdoc />
    public async Task<bool> OrganizationExistAsync(
        string externalId,
        string name,
        string displayName,
        bool ignoreCase = true,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(externalId)
            && string.IsNullOrWhiteSpace(displayName)
            && string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("External Id, displayName and name of organization should not be all null or whitespace");
        }

        string collectionName = _modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>();
        ParameterizedAql aqlQuery = WellKnownFirstLevelProjectionQueries.OrganizationExist(
            collectionName,
            externalId,
            name,
            displayName,ignoreCase);

        IList<bool> result = await ExecuteQueryAsync<bool>(aqlQuery, null, cancellationToken);

        return Logger.ExitMethod(result.FirstOrDefault());
    }


    /// <inheritdoc />
    public Task DeleteProfileAssignmentAsync(
        string parentId,
        ContainerType parentType,
        string profileId,
        IList<RangeCondition> conditions,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (parentId == null)
        {
            throw new ArgumentNullException(nameof(parentId));
        }

        if (conditions == null)
        {
            throw new ArgumentNullException(nameof(conditions));
        }

        if (string.IsNullOrWhiteSpace(parentId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentId));
        }

        if (!Enum.IsDefined(typeof(ContainerType), parentType))
        {
            throw new InvalidEnumArgumentException(nameof(parentType), (int)parentType, typeof(ContainerType));
        }

        if (parentType == ContainerType.NotSpecified)
        {
            throw new ArgumentException(
                "NotSpecified is not allowed when creating an assignment",
                nameof(parentType));
        }

        if (conditions.Count == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(conditions));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => DeleteProfileAssignmentInternalAsync(
                parentId,
                parentType.ToFirstLevelEntityType(),
                profileId,
                conditions,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task RemoveTagFromProfileAsync(
        string tagId,
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagId == null)
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => RemoveTagInternalAsync<IFirstLevelProjectionProfile>(
                tagId,
                profileId,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task RemoveTagFromRoleAsync(
        string tagId,
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagId == null)
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        if (roleId == null)
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(roleId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => RemoveTagInternalAsync<FirstLevelProjectionRole>(
                tagId,
                roleId,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task RemoveTagFromFunctionAsync(
        string tagId,
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tagId == null)
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        if (functionId == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(functionId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => RemoveTagInternalAsync<FirstLevelProjectionFunction>(
                tagId,
                functionId,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task SetClientSettingsAsync(
        string profileId,
        string clientSetting,
        string key,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (clientSetting == null)
        {
            throw new ArgumentNullException(nameof(clientSetting));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(clientSetting))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientSetting));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => SetClientSettingsInternalAsync(
                profileId,
                clientSetting,
                key,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task UnsetClientSettingsAsync(
        string profileId,
        string key,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task task = arangoTransaction.ExecuteWithLock(
            () => UnsetClientSettingsInternalAsync(
                profileId,
                key,
                arangoTransaction,
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default)
    {
        Logger.EnterMethod();

        Task<Dictionary<string, ulong>> task =
            _projectionStateRepository.GetLatestProjectedEventIdsAsync(stoppingToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task SaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Task task =
            _projectionStateRepository.SaveProjectionStateAsync(projectionState, transaction, cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public async Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        return Logger.ExitMethod(
            await _projectionStateRepository.GetPositionOfLatestProjectedEventAsync(cancellationToken));
    }
}
