using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.Client.ArangoDb.Public.Models.Transaction;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using AVMember = Maverick.UserProfileService.Models.Models.Member;
using AVRangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;
using AVTagType = Maverick.UserProfileService.Models.EnumModels.TagType;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An Implementation of <see cref="ISecondLevelProjectionRepository" /> for the Arango DB.
/// </summary>
public class ArangoSecondLevelProjectionRepository : ArangoRepositoryBase, ISecondLevelProjectionRepository
{
    /// <summary>
    ///     A <see cref="IMapper" /> used to map Aggregate models to AV360 models and vice versa.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    ///     Holds the information which model should be stored in which database.
    /// </summary>
    private readonly ModelBuilderOptions _modelsInfo;

    /// <summary>
    ///     The name of the collection where the edges of tree graph (of each entity) are saved.
    /// </summary>
    private readonly string _pathTreeEdgeCollectionName;

    /// <summary>
    ///     The name of the collection where the vertices of tree graph (of each entity) are saved.
    /// </summary>
    private readonly string _pathTreeVertexCollectionName;

    /// <summary>
    ///     All operations of <see cref="IProjectionStateRepository" /> will be forwarded to this instance.
    /// </summary>
    private readonly ArangoProjectionStateRepository _projectionStateRepository;

    /// <summary>
    ///     JsonSerializer containing the default deserializer settings of the arango client
    /// </summary>
    private readonly JsonSerializer _serializer;

    /// <summary>
    ///     JsonSerializer settings containing the default deserializer settings of the arango client
    /// </summary>
    private readonly JsonSerializerSettings _serializerSettings;

    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ArangoSecondLevelProjectionRepository" />.
    /// </summary>
    /// <param name="logger">The logger instance to be used by the new instance.</param>
    /// <param name="serviceProvider">
    ///     The service provider that contains all registered services. It is used to initialize new
    ///     instances of <see cref="ArangoProjectionStateRepository" />.
    /// </param>
    /// <param name="mapper">A &lt;see cref="IMapper" /&gt; used to map Aggregate models to AV360 models and vice versa.</param>
    /// <param name="collectionPrefix">The second level collection prefix.</param>
    /// <param name="arangoDbClientName">The name of the arango client to be used by the new instance.</param>
    public ArangoSecondLevelProjectionRepository(
        ILogger<ArangoSecondLevelProjectionRepository> logger,
        IServiceProvider serviceProvider,
        IMapper mapper,
        string collectionPrefix,
        string arangoDbClientName = null) : base(logger, serviceProvider)
    {
        _modelsInfo = DefaultModelConstellation.CreateNewSecondLevelProjection(collectionPrefix).ModelsInfo;
        _mapper = mapper;

        _projectionStateRepository = new ArangoProjectionStateRepository(
            logger,
            serviceProvider,
            arangoDbClientName,
            _modelsInfo.GetCollectionName<ProjectionState>());

        if (arangoDbClientName != null)
        {
            ArangoDbClientName = arangoDbClientName;
        }

        _serializerSettings = GetArangoDbClient()?.UsedJsonSerializerSettings;
        _serializer = JsonSerializer.Create(_serializerSettings);

        _pathTreeEdgeCollectionName = _modelsInfo
            .GetRelation<SecondLevelProjectionProfileVertexData,
                SecondLevelProjectionProfileVertexData>()
            .EdgeCollection;

        _pathTreeVertexCollectionName = GetCollectionName<SecondLevelProjectionProfileVertexData>();
    }

    /// <summary>
    ///     Adds a memberOf entry <paramref name="container" /> to a profile in the database. Existing entries won't be
    ///     replaced.
    /// </summary>
    /// <exception cref="ArgumentException">The container type is wrong. Groups and organizations are supported.</exception>
    /// <exception cref="DatabaseException">The wrong amount of profiles have been updated - either less or more than 1.</exception>
    private async Task AddMemberOfToProfileInternalAsync(
        string profileId,
        ISecondLevelProjectionContainer container,
        IList<RangeCondition> conditions,
        string transactionId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (container.ContainerType != ContainerType.Group
            && container.ContainerType != ContainerType.Organization)
        {
            throw new ArgumentException(
                "Wrong container type. Only groups and organizations are supported by this method.");
        }

        AVMember convertedContainer = _mapper.MapContainerTypeToAvMember(container);

        convertedContainer.Conditions = conditions
            .Select(
                c => new AVRangeCondition
                {
                    Start = c.Start,
                    End = c.End
                })
            .ToList();

        ParameterizedAql addMemberOfQuery = WellKnownSecondLevelProjectionQueries
            .AddMemberOfSetToProfile(
                profileId,
                _modelsInfo,
                _serializer,
                convertedContainer);

        IReadOnlyList<IProfileEntityModel> response = await ExecuteAqlQueryAsync<IProfileEntityModel>(
            addMemberOfQuery,
            transactionId,
            cancellationToken: cancellationToken);

        if (response.Count != 1)
        {
            throw new DatabaseException(
                $"Could not update memberOf field of profile '{profileId}'",
                ExceptionSeverity.Error);
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Updated memberOf field of profile {profileId} - new data: {memberOfData}",
                LogHelpers.Arguments(profileId, response.Single().MemberOf.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Updated memberOf field of profile {profileId}",
                profileId.AsArgumentList());
        }

        Logger.ExitMethod();
    }

    /// <summary>
    ///     Adds an entry <paramref name="container" /> to the security assignment set of a profile in the database. Existing
    ///     entries won't be replaced.
    /// </summary>
    /// <exception cref="ArgumentException">The container type is wrong. Roles and functions are supported.</exception>
    /// <exception cref="DatabaseException">The wrong amount of profiles have been updated - either less or more than 1.</exception>
    private async Task AddSecurityAssignmentLinkToProfileInternalAsync(
        string profileId,
        ISecondLevelProjectionContainer container,
        IList<RangeCondition> conditions,
        string transactionId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (container.ContainerType != ContainerType.Function
            && container.ContainerType != ContainerType.Role)
        {
            throw new ArgumentException("Wrong container type. Only roles and functions are supported by this method.");
        }

        ILinkedObject convertedRoleOrFunctionContainer =
            _mapper.MapContainerTypeToLinkedObject(container);

        convertedRoleOrFunctionContainer.Conditions = conditions
            .Select(
                c => new AVRangeCondition
                {
                    Start = c.Start,
                    End = c.End
                })
            .ToList();

        ParameterizedAql addMemberOfQuery = WellKnownSecondLevelProjectionQueries
            .AddSecurityAssignmentEntriesToProfile(
                profileId,
                _modelsInfo,
                _serializer,
                convertedRoleOrFunctionContainer);

        IReadOnlyList<IProfileEntityModel> response = await ExecuteAqlQueryAsync<IProfileEntityModel>(
            addMemberOfQuery,
            transactionId,
            cancellationToken: cancellationToken);

        if (response.Count != 1)
        {
            throw new DatabaseException(
                $"Could not update security assignment field of profile '{profileId}' properly",
                ExceptionSeverity.Error);
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Updated security assignment field of profile {profileId} - new data: {memberOfData}",
                LogHelpers.Arguments(profileId, response.Single().MemberOf.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Updated security assignment field of profile {profileId}",
                profileId.AsArgumentList());
        }

        Logger.ExitMethod();
    }

    private async Task<DeleteDocumentResponse> DeleteEntityAsync<TEntity>(
        string entityId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        Logger.LogInfoMessage("Deleting {entity} with id {id}", Arguments(typeof(TEntity).Name, entityId));

        cancellationToken.ThrowIfCancellationRequested();

        string documentId = GetDocumentId(GetCollectionName<TEntity>(), entityId);
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        //TODO: looks better delete options parameters
        DeleteDocumentResponse response =
            await GetArangoDbClient().DeleteDocumentAsync(documentId, null, transactionId);

        return Logger.ExitMethod(response);
    }

    protected string GetCollectionName<T>()
    {
        return _modelsInfo.GetCollectionName<T>();
    }

    private static string GetDocumentId(string collectionName, string objectId)
    {
        return $"{collectionName}/{objectId}";
    }

    // Todo Can this method be removed?
    private async Task RecalculateTagsOfProfile(
        string profileId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();
        IList<CalculatedTag> calculatedTags = await CalculateTagsAsync(profileId, transactionId, cancellationToken);
        JArray newTags = JArray.FromObject(calculatedTags, _serializer);
        string collectionName = GetCollectionName<IProfileEntityModel>();

        var updateQuery = @$" FOR x in {
            collectionName
        }
                    FILTER x.RelatedProfileId == ""{
                        profileId
                    }""  
                    UPDATE x WITH {{Tags: {
                        newTags
                    }}}
                    IN {
                        collectionName
                    } RETURN NEW";

        MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
            .ExecuteQueryAsync<IProfileEntityModel>(
                updateQuery,
                transactionId,
                cancellationToken: cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by updating tags of the profile (Id = {profileId}",
                LogHelpers.Arguments(profileId));
        }
        else
        {
            Logger.LogDebugMessage(
                "Tags of the profile (Id = {profileId} have been successfully updated",
                LogHelpers.Arguments(profileId));
        }
    }

    private async Task<IList<CalculatedTag>> CalculateTagsAsync(
        string profileId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string rootVertex =
            WellKnownSecondLevelProjectionQueries.GetRootVertexDocumentId(_pathTreeVertexCollectionName, profileId);

        IReadOnlyList<CalculatedTag> calculatedTags = await ExecuteAqlQueryAsync<CalculatedTag>(
            WellKnownSecondLevelProjectionQueries
                .CalculateTags(
                    rootVertex,
                    profileId,
                    _modelsInfo),
            transactionId,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(calculatedTags?.ToList() ?? new List<CalculatedTag>());
    }

    private async Task<List<string>> CalculatePathInternalAsync(
        string relatedProfileId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        string rootVertex = WellKnownSecondLevelProjectionQueries.GetRootVertexDocumentId(
            _pathTreeVertexCollectionName,
            relatedProfileId);

        IReadOnlyList<string> calculatedPaths = await ExecuteAqlQueryAsync<string>(
            WellKnownSecondLevelProjectionQueries
                .CalculatePath(
                    rootVertex,
                    relatedProfileId,
                    _modelsInfo),
            transactionId,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(calculatedPaths?.ToList() ?? new List<string>());
    }

    /// <summary>
    ///     Validates the given <see cref="IDatabaseTransaction" /> and throws an exception if the exception is not valid.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <returns>The <see cref="ArangoTransaction" /> which was passed.</returns>
    /// <exception cref="ArgumentException">Will be yielded if the transaction is not suitable for this repository.</exception>
    protected ArangoTransaction ValidateTransaction(IDatabaseTransaction transaction)
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
                "The passed transaction is not suited for ArangoDB operations. All second level projection collections have to be included within the transaction.",
                nameof(transaction));
        }

        return Logger.ExitMethod(arangoTransaction);
    }

    protected async Task<CreateDocumentResponse> CreateEntityInternalAsync<TEntity>(
        TEntity entity,
        string entityId = null,
        bool withResponseCheck = true,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)

    {
        Logger.EnterMethod();

        string collectionName = GetCollectionName<TEntity>();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        if (transactionId == null)
        {
            Logger.LogDebugMessage(
                "Inserting entity of type: {entityType} with the id: {profileId} in the collection: {collectionName}",
                LogHelpers.Arguments(typeof(TEntity), entityId, collectionName));
        }
        else
        {
            Logger.LogDebugMessage(
                "Inserting entity of type :{entityType} with id: {profileId} in the collection {collectionName} inside the transaction: {transactionId}.",
                LogHelpers.Arguments(typeof(TEntity), entityId, collectionName, transactionId));
        }

        CreateDocumentResponse response;

        if (entityId != null)
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
                    },
                    transactionId);
        }
        else
        {
            JObject toSavedEntity = JObject.FromObject(entity, _serializer);

            response = await GetArangoDbClient()
                .CreateDocumentAsync(
                    collectionName,
                    toSavedEntity,
                    new CreateDocumentOptions
                    {
                        Overwrite = false,
                        ReturnNew = true,
                        ReturnOld = true,
                        OverWriteMode = AOverwriteMode.Conflict,
                        WaitForSync = true
                    },
                    transactionId);
        }

        if (response == null)
        {
            return null;
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
                    context: transaction?.CallingService,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return Logger.ExitMethod(response);
    }

    private static bool IsObjectTypeProfileEntity(ObjectType objectType)
    {
        return objectType is ObjectType.User or ObjectType.Profile or ObjectType.Group or ObjectType.Organization;
    }

    private async Task AddTagFromProfileInEntityTree(
        string relatedProfileId,
        string profileId,
        IEnumerable<TagAssignment> tags,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;
        string collectionName = GetCollectionName<SecondLevelProjectionProfileVertexData>();

        JArray newTags = JArray.FromObject(tags, _serializer);

        var query = @$" FOR x in {
            collectionName
        }
                    FILTER x.RelatedProfileId == ""{
                        relatedProfileId
                    }"" AND x.ObjectId == ""{
                        profileId
                    }""   
                    LET tags = (x.Tags == null ? {
                        newTags
                    } : APPEND(x.Tags,{
                        newTags
                    }))
                    UPDATE x WITH {{Tags: tags}}
                    IN {
                        collectionName
                    }
                    RETURN NEW";

        Logger.LogDebugMessage(
            "Adding Tags inside the vertex of the entity (Id = {profileId}) in the tree of the entity (Id = {relatedProfileId}",
            LogHelpers.Arguments(profileId, relatedProfileId));

        MultiApiResponse<SecondLevelProjectionProfileVertexData> response = await GetArangoDbClient()
            .ExecuteQueryAsync<SecondLevelProjectionProfileVertexData>(
                query,
                transactionId,
                cancellationToken: cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by updating tags of the profile (Id = {profileId}",
                LogHelpers.Arguments(profileId));
        }
        else
        {
            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Tags ({tags}) of the profile (Id = {profileId} have been successfully updated",
                    LogHelpers.Arguments(newTags.ToLogString(), profileId));
            }
            else
            {
                Logger.LogInfoMessage(
                    "Tags of the profile (Id = {profileId} have been successfully updated",
                    LogHelpers.Arguments(profileId));
            }
        }

        Logger.ExitMethod();
    }

    private async Task RemoveTagFromProfileInEntityTree(
        string relatedProfileId,
        string profileId,
        IEnumerable<string> tags,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;
        string collectionName = GetCollectionName<SecondLevelProjectionProfileVertexData>();

        JArray oldTagsIdList = JArray.FromObject(tags, _serializer);

        var query = @$"
                    WITH {
                        collectionName
                    }
                    FOR node IN {
                        collectionName
                    }
                    FILTER node.{
                        nameof(SecondLevelProjectionProfileVertexData.RelatedProfileId)
                    } == ""{
                        relatedProfileId
                    }""
                       AND node.{
                           nameof(SecondLevelProjectionProfileVertexData.ObjectId)
                       } == ""{
                           profileId
                       }""
                    LET toDeleteTags = (
                                         FOR tag IN node.{
                                             nameof(SecondLevelProjectionProfileVertexData.Tags)
                                         }
                                         FILTER tag.{
                                             nameof(TagAssignment.TagDetails)
                                         } != null 
                                            AND tag.{
                                                nameof(TagAssignment.TagDetails)
                                            }.{
                                                nameof(TagAssignment.TagDetails.Id)
                                            } 
                                                IN {
                                                    oldTagsIdList
                                                }
                                         RETURN tag.{
                                             nameof(TagAssignment.TagDetails)
                                         }.{
                                             nameof(TagAssignment.TagDetails.Id)
                                         }
                                       )
                    LET newTags = (
                                   node.Tags == null
                                      ? node.Tags
                                      : node.Tags[* FILTER CURRENT.{
                                          nameof(TagAssignment.TagDetails)
                                      } != null
                                                        AND toDeleteTags ANY != CURRENT.{
                                                            nameof(TagAssignment.TagDetails)
                                                        }.{
                                                            nameof(TagAssignment.TagDetails.Id)
                                                        }]
                                  )
                    UPDATE node WITH {{ {
                        nameof(SecondLevelProjectionProfileVertexData.Tags)
                    }: newTags }}
                    IN {
                        collectionName
                    } RETURN NEW";

        Logger.LogDebugMessage(
            "Removing Tags inside the vertex of the entity (Id = {profileId}) in the tree of the entity (Id = {relatedProfileId}",
            LogHelpers.Arguments(profileId, relatedProfileId));

        IReadOnlyList<string> modifiedNode = await ExecuteAqlQueryAsync<string>(
            new ParameterizedAql
            {
                Query = query
            },
            transactionId,
            true,
            true,
            cancellationToken);

        if (modifiedNode == null || modifiedNode.Count == 0)
        {
            Logger.LogWarnMessage(
                "Error by removing tags ({tags}) of the profile (Id = {profileId}",
                LogHelpers.Arguments(tags.ToLogString(), profileId));
        }
        else
        {
            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Tags ({tags}) of the profile (Id = {profileId} have been successfully removed",
                    LogHelpers.Arguments(tags.ToLogString(), profileId));
            }
            else
            {
                Logger.LogInfoMessage(
                    "Tags of the profile (Id = {profileId} have been successfully removed",
                    LogHelpers.Arguments(profileId));
            }
        }

        Logger.ExitMethod();
    }

    private async Task RecalculatePathsAndTagsOfProfileAsync(
        string profileId,
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "Calculating paths for the profile: {relatedProfileId}.",
            LogHelpers.Arguments(profileId));

        List<string> calculatedPaths =
            await CalculatePathInternalAsync(profileId, transactionId, cancellationToken);

        if (calculatedPaths != null && calculatedPaths.Any())
        {
            Logger.LogDebugMessage(
                "Updating paths for the profile: {relatedProfileId} after calculation.",
                LogHelpers.Arguments(profileId));

            await UpdatePathInProfileAsync(profileId, calculatedPaths, transactionId, cancellationToken);
        }
        else
        {
            Logger.LogWarnMessage(
                "Error happened by calculating the new paths of the profile {relatedProfileId}",
                LogHelpers.Arguments(profileId));
        }

        Logger.LogDebugMessage(
            "Calculating tags for the profile: {relatedProfileId}.",
            LogHelpers.Arguments(profileId));

        IList<CalculatedTag> calculatedTags = await CalculateTagsAsync(profileId, transactionId, cancellationToken);

        Logger.LogDebugMessage(
            "Updating tags for the profile: {relatedProfileId} after calculation.",
            LogHelpers.Arguments(profileId));

        await UpdateTagsInProfileAsync(profileId, calculatedTags, transactionId, cancellationToken);

        Logger.ExitMethod();
    }

    private async Task UpdatePathInProfileAsync(
        string profileId,
        IList<string> paths,
        string transactionId = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        JArray newPaths = JArray.FromObject(paths, _serializer);
        string collectionName = GetCollectionName<IProfileEntityModel>();

        var query = @$" FOR x in {
            collectionName
        }
                    FILTER x.Id == ""{
                        profileId
                    }""                  
                    UPDATE x WITH {{Paths: {
                        newPaths
                    }}}
                    IN {
                        collectionName
                    } RETURN NEW";

        MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
            .ExecuteQueryAsync<IProfileEntityModel>(
                query,
                transactionId,
                cancellationToken: cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by updating paths of the profile (Id = {profileId}",
                LogHelpers.Arguments(profileId));
        }
        else
        {
            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Paths ({paths}) of the profile (Id = {profileId} have been successfully updated",
                    LogHelpers.Arguments(newPaths.ToLogString(), profileId));
            }
            else
            {
                Logger.LogInfoMessage(
                    "Paths of the profile (Id = {profileId} have been successfully updated",
                    LogHelpers.Arguments(profileId));
            }
        }

        Logger.ExitMethod();
    }

    private async Task UpdateTagsInProfileAsync(
        string profileId,
        IList<CalculatedTag> tags,
        string transactionId = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        JArray newTags = JArray.FromObject(tags, _serializer);
        string collectionName = GetCollectionName<IProfileEntityModel>();

        var query = @$" FOR x in {
            collectionName
        }
                    FILTER x.Id == ""{
                        profileId
                    }""                  
                    UPDATE x WITH {{Tags: {
                        newTags
                    }}}
                    IN {
                        collectionName
                    } RETURN NEW";

        MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
            .ExecuteQueryAsync<IProfileEntityModel>(
                query,
                transactionId,
                cancellationToken: cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by updating tags of the profile (Id = {profileId}",
                LogHelpers.Arguments(profileId));
        }
        else
        {
            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Tags ({tags}) of the profile (Id = {profileId} have been successfully updated",
                    LogHelpers.Arguments(newTags.ToLogString(), profileId));
            }
            else
            {
                Logger.LogInfoMessage(
                    "Tags of the profile (Id = {profileId} have been successfully updated",
                    LogHelpers.Arguments(profileId));
            }
        }

        Logger.ExitMethod();
    }

    private async Task DeleteEntityInPathTree(
        string entityId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();
        string verticesCollectionName = GetCollectionName<SecondLevelProjectionProfileVertexData>();

        string edgesCollectionName = _modelsInfo
            .GetRelation<SecondLevelProjectionProfileVertexData,
                SecondLevelProjectionProfileVertexData>()
            .EdgeCollection;

        var deleteVerticesQuery =
            $@"FOR x IN {
                verticesCollectionName
            } 
                FILTER x.{
                    nameof(SecondLevelProjectionProfileVertexData.RelatedProfileId)
                } == ""{
                    entityId
                }"" 
                OR x.{
                    nameof(SecondLevelProjectionProfileVertexData.ObjectId)
                } == ""{
                    entityId
                }"" 
                REMOVE x IN {
                    verticesCollectionName
                } RETURN OLD";

        MultiApiResponse<SecondLevelProjectionProfileVertexData> deleteVerticesResponse =
            await GetArangoDbClient()
                .ExecuteQueryAsync<SecondLevelProjectionProfileVertexData>(
                    deleteVerticesQuery,
                    transactionId,
                    cancellationToken: cancellationToken);

        if (deleteVerticesResponse == null || deleteVerticesResponse.Error)
        {
            Logger.LogWarnMessage(
                "Error by deleting all related vertices to the entity: {entityId} ",
                LogHelpers.Arguments(entityId));
        }

        // Todo: Query still necessary or missing usage of it?
        var deleteEdgesQuery =
            $@"FOR x IN {
                edgesCollectionName
            } 
                FILTER x.{
                    nameof(SecondLevelProjectionProfileEdgeData.RelatedProfileId)
                } == ""{
                    entityId
                }"" 
                OR CONTAINS(x._from, ""{
                    entityId
                }"")
                OR CONTAINS(x._to, ""{
                    entityId
                }"")
                REMOVE x IN {
                    edgesCollectionName
                } RETURN OLD ";

        MultiApiResponse<SecondLevelProjectionProfileVertexData> deleteEdgesResponse =
            await GetArangoDbClient()
                .ExecuteQueryAsync<SecondLevelProjectionProfileVertexData>(
                    deleteEdgesQuery,
                    transactionId,
                    cancellationToken: cancellationToken);

        if (deleteEdgesResponse == null || deleteEdgesResponse.Error)
        {
            Logger.LogWarnMessage(
                "Error by deleting all related edges to the entity: {entityId} ",
                LogHelpers.Arguments(entityId));
        }

        Logger.ExitMethod();
    }

    private async Task<IReadOnlyList<TResultItem>> ExecuteAqlQueryAsync<TResultItem>(
        ParameterizedAql aql,
        string transactionId = null,
        bool throwException = true,
        bool throwExceptionIfNotFound = false,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
    {
        Logger.EnterMethod();

        var cursorBody = new CreateCursorBody
        {
            Query = aql.Query,
            BindVars = aql.Parameter ?? new Dictionary<string, object>()
        };

        if (Logger.IsEnabledFor(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "Executing AQL query (in behalf of {behalfOf}): {aql}",
                Arguments(caller, aql.Query));
        }

        // maybe sensitive data - who knows?!
        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Used parameter set of AQL query (in behalf of {behalfOf}): {aqlParameter}",
                Arguments(caller, aql.Parameter.ToLogString()));
        }

        cancellationToken.ThrowIfCancellationRequested();

        MultiApiResponse<TResultItem> response = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<TResultItem>(
                cursorBody,
                transactionId,
                cancellationToken: cancellationToken),
            throwException,
            throwExceptionIfNotFound,
            CallingServiceContext.CreateNewOf<ArangoSecondLevelProjectionRepository>(),
            cancellationToken);

        return Logger.ExitMethod(response.QueryResult);
    }

    private static bool IsRangeConditionActive(AVRangeCondition condition)
    {
        return (condition.Start == null || condition.Start < DateTime.UtcNow)
            && (condition.End == null || condition.End > DateTime.UtcNow);
    }

    /// <inheritdoc />
    public async Task<IDatabaseTransaction> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage("Starting transaction", Arguments());

        IList<string> collections = _modelsInfo.GetDocumentCollections()
            .Union(_modelsInfo.GetEdgeCollections())
            .Where(c => c != null)
            .ToList();

        TransactionOperationResponse result = await GetArangoDbClient()
            .BeginTransactionAsync(
                collections,
                collections)
            .ConfigureAwait(false);

        await CheckAResponseAsync(
                result,
                context: CallingServiceContext.CreateNewOf<ArangoSecondLevelProjectionRepository>(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        string transactionId = result.GetTransactionId();

        Logger.LogInfoMessage("Started transaction {id}.", Arguments(transactionId));

        IDatabaseTransaction transaction = new ArangoTransaction
        {
            TransactionId = transactionId,
            Collections = collections
        };

        return Logger.ExitMethod(transaction);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ArangoTransaction convertedTransaction = ValidateTransaction(transaction);

        TransactionOperationResponse result = await GetArangoDbClient()
            .CommitTransactionAsync(convertedTransaction?.TransactionId)
            .ConfigureAwait(false);

        convertedTransaction?.MarkAsInactive();

        await CheckAResponseAsync(
                result,
                context: transaction.CallingService,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Logger.LogInfoMessage(
            "Committed transaction {transactionId}.",
            Arguments(convertedTransaction?.TransactionId));

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task SetClientSettingsAsync(
        string profileId,
        string key,
        string settings,
        bool isInherited,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Parameters: profileId: {profileId}; key: {settingKey}; settings: {settingsValue}; isInherited: {isInherited}",
                Arguments(profileId, key, settings, isInherited));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;
        string collectionName = _modelsInfo.GetCollectionName<IProfileEntityModel>();

        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new DatabaseException(
                $"Could not find a collection name for entity type {nameof(IProfileEntityModel)}",
                ExceptionSeverity.Error);
        }

        cancellationToken.ThrowIfCancellationRequested();

        ParameterizedAql paramAql = WellKnownSecondLevelProjectionQueries
            .SetClientSettingsKey(
                new ClientSettingsEntityModel
                {
                    ProfileId = profileId,
                    Value = JObject.Parse(settings),
                    IsInherited = isInherited,
                    SettingsKey = key
                },
                _modelsInfo);

        IReadOnlyList<string> result = await ExecuteAqlQueryAsync<string>(
            paramAql,
            transactionId,
            cancellationToken: cancellationToken);

        Logger.LogInfoMessage(
            "Stored client settings entry as new document in database (profileId: {profileId}; key: {settingKey}).",
            Arguments(profileId, key));

        if (result.Count > 1)
        {
            Logger.LogWarnMessage(
                "More than one entry has been replace by the database system (amount: {replacedAmount}). This indicates an issue with the stored data.",
                Arguments(result.Count));
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task UnsetClientSettingFromProfileAsync(
        string profileId,
        string key,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Parameters: profileId: {profileId}; key: {settingKey}",
                Arguments(profileId, key));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<string> queryResult = await ExecuteAqlQueryAsync<string>(
            WellKnownSecondLevelProjectionQueries.UnsetClientSettingsKey(
                profileId,
                key,
                _modelsInfo),
            transactionId,
            cancellationToken: cancellationToken);

        if (queryResult.Count == 0)
        {
            throw new InstanceNotFoundException(
                "CLIENT_SETTINGS_NOT_FOUND",
                $"profileId={profileId},key={key}",
                $"Could not remove client settings entry. The key {key} was not found for profile {profileId}.");
        }

        Logger.LogInfoMessage(
            "Removed client settings entry in database (profileId: {profileId}; key: {settingKey}).",
            Arguments(profileId, key));

        if (queryResult.Count > 1)
        {
            Logger.LogWarnMessage(
                "Deleted more than one entry (amount: {deletedAmount}). That is not problem at the moment, but indicates a different issue while storing client settings.",
                Arguments(queryResult.Count));
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task InvalidateClientSettingsFromProfile(
        string profileId,
        string[] remainingKeys,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Parameters: profileId: {profileId}; remainingKeys: {remainingSettingKeys}",
                Arguments(profileId, remainingKeys.ToLogString()));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<string> deletedKeys = await ExecuteAqlQueryAsync<string>(
            WellKnownSecondLevelProjectionQueries.InvalidateClientSettingsKey(
                profileId,
                remainingKeys,
                _modelsInfo),
            transactionId,
            cancellationToken: cancellationToken);

        if (deletedKeys.Count == 0)
        {
            Logger.LogInfoMessage(
                "No key matches the invalidation request. No key has been deleted (profileId: {profileId}).",
                Arguments(profileId));
        }
        else
        {
            Logger.LogInfoMessage(
                "Removed {removedAmount} client settings entries in database (profileId: {profileId}; keys: {removedSettingKeys}).",
                Arguments(
                    deletedKeys.Count,
                    profileId,
                    deletedKeys.ToLogString()));
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);
        string transactionId = arangoTransaction.TransactionId;

        try
        {
            TransactionOperationResponse result = await GetArangoDbClient()
                .AbortTransactionAsync(transactionId)
                .ConfigureAwait(false);

            arangoTransaction.MarkAsInactive();

            await CheckAResponseAsync(
                    result,
                    context: transaction.CallingService,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            Logger.LogInfoMessage("Aborted transaction {id}.", Arguments(transactionId));
        }
        catch (Exception ex)
        {
            Logger.LogErrorMessage(
                ex,
                "The transaction with the id = {id} could not be aborted.",
                LogHelpers.Arguments(transactionId));
        }

        Logger.ExitMethod();
    }
    
    /// <inheritdoc />
    public async Task<ISecondLevelProjectionProfile> GetProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        string documentId = GetDocumentId(GetCollectionName<IProfileEntityModel>(), profileId);
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        GetDocumentResponse<IProfileEntityModel> response =
            await GetArangoDbClient().GetDocumentAsync<IProfileEntityModel>(documentId, transactionId);

        IProfileEntityModel profile = response?.Result;

        if (profile == null)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No profile found with id '{profileId}'.");
        }

        ISecondLevelProjectionProfile result = _mapper.MapProfile(profile);

        Logger.LogDebugMessage("Found profile with id {profileId}.", LogHelpers.Arguments(profileId));

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IList<string>> GetPathOfProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        List<string> paths = await CalculatePathInternalAsync(
            profileId,
            transactionId,
            cancellationToken);

        return Logger.ExitMethod(paths);
    }

    /// <inheritdoc />
    public async Task<SecondLevelProjectionFunction> GetFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        string documentId = GetDocumentId(GetCollectionName<FunctionObjectEntityModel>(), functionId);
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        GetDocumentResponse<FunctionObjectEntityModel> response = await GetArangoDbClient()
            .GetDocumentAsync<FunctionObjectEntityModel>(documentId, transactionId);

        if (response?.Result == null)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.RoleOrFunctionNotFoundString,
                $"No profile found with id '{functionId}'.");
        }

        Logger.LogDebugMessage("Found profile with id {profileId}.", LogHelpers.Arguments(functionId));

        SecondLevelProjectionFunction result =
            _mapper.Map<FunctionObjectEntityModel, SecondLevelProjectionFunction>(response.Result);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<SecondLevelProjectionRole> GetRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        string documentId = GetDocumentId(GetCollectionName<RoleObjectEntityModel>(), roleId);
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        GetDocumentResponse<RoleObjectEntityModel> response =
            await GetArangoDbClient().GetDocumentAsync<RoleObjectEntityModel>(documentId, transactionId);

        if (response?.Result == null)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.RoleOrFunctionNotFoundString,
                $"No profile found with id '{roleId}'.");
        }

        Logger.LogDebugMessage("Found profile with id {profileId}.", LogHelpers.Arguments(roleId));

        SecondLevelProjectionRole result =
            _mapper.Map<RoleObjectEntityModel, SecondLevelProjectionRole>(response.Result);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task CreateProfileAsync(
        ISecondLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (profile.Id == null)
        {
            throw new ArgumentNullException(nameof(profile.Id));
        }

        IProfileEntityModel convertedProfile = _mapper.MapProfile(profile);

        if (profile.Paths == null || !profile.Paths.Any())
        {
            convertedProfile.Paths = new List<string>
            {
                profile.Id
            };
        }

        CreateDocumentResponse response = await CreateEntityInternalAsync(
            convertedProfile,
            convertedProfile.Id,
            true,
            transaction,
            cancellationToken);

        if (response == null || response.Error)
        {
            Logger.LogWarnMessage(
                "Error by creating a new profile with (Id = {entityId})",
                LogHelpers.Arguments(profile.Id));

            return;
        }

        var vertexData = new SecondLevelProjectionProfileVertexData
        {
            RelatedProfileId = profile.Id,
            ObjectId = profile.Id
        };

        Logger.LogDebugMessage(
            "Creating a new vertex in the graph of profile {entityId}",
            LogHelpers.Arguments(profile.Id));

        CreateDocumentResponse result = await CreateEntityInternalAsync(
            vertexData,
            WellKnownSecondLevelProjectionQueries.GetVertexId(vertexData),
            true,
            transaction,
            cancellationToken);

        if (result == null || result.Error)
        {
            Logger.LogWarnMessage(
                "Error by creating a new vertex in the graph of profile {entityId}",
                LogHelpers.Arguments(profile.Id));
        }

        await CheckAResponseAsync(
                result,
                true,
                context: transaction?.CallingService,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task CreateFunctionAsync(
        SecondLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        string functionId = function.Id;

        if (function.Id == null)
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        FunctionObjectEntityModel convertedFunction =
            _mapper.Map<SecondLevelProjectionFunction, FunctionObjectEntityModel>(function);

        convertedFunction.Name = convertedFunction.GenerateFunctionModelName();

        await CreateEntityInternalAsync(
            convertedFunction,
            functionId,
            true,
            transaction,
            cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task CreateRoleAsync(
        SecondLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (role.Id == null)
        {
            throw new ArgumentNullException(nameof(role.Id));
        }

        string roleId = role.Id;

        RoleObjectEntityModel convertedRole = _mapper.Map<SecondLevelProjectionRole, RoleObjectEntityModel>(role);

        CreateDocumentResponse response = await CreateEntityInternalAsync(
            convertedRole,
            roleId,
            true,
            transaction,
            cancellationToken);

        Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task CreateTagAsync(
        Tag tag,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        if (tag.Id == null)
        {
            throw new ArgumentNullException(nameof(tag.Id));
        }

        string tagId = tag.Id;

        CreateDocumentResponse response = await CreateEntityInternalAsync(
            tag,
            tagId,
            true,
            transaction,
            cancellationToken);

        Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task AddTagToObjectAsync(
        string relatedObjectId,
        string objectId,
        ObjectType objectType,
        IEnumerable<TagAssignment> tags,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (relatedObjectId == null)
        {
            throw new ArgumentNullException(nameof(relatedObjectId));
        }

        if (objectId == null)
        {
            throw new ArgumentNullException(nameof(objectId));
        }

        List<TagAssignment> tagAssignments = tags as List<TagAssignment> ?? tags?.ToList();

        if (tagAssignments == null || !tagAssignments.Any())
        {
            Logger.LogWarnMessage(
                "Can't add tags to object: {objectId}, the tag list was leer or empty",
                LogHelpers.Arguments(objectId));

            return;
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        if (relatedObjectId == objectId)
        {
            IEnumerable<CalculatedTag> calculatedTags = tagAssignments.Select(
                t => new CalculatedTag
                {
                    Id = t?.TagDetails?.Id,
                    Name = t?.TagDetails?.Name,
                    Type = _mapper.Map<AVTagType>(t?.TagDetails?.Type),
                    IsInherited = false
                });

            JArray tagArray = JArray.FromObject(calculatedTags, _serializer);
            string collectionName;

            //TODO: create a separate method
            switch (objectType)
            {
                case ObjectType.User:
                case ObjectType.Group:
                case ObjectType.Organization:
                case ObjectType.Profile:
                    collectionName = GetCollectionName<IProfileEntityModel>();

                    break;
                case ObjectType.Function:
                case ObjectType.Role:
                    collectionName = GetCollectionName<FunctionObjectEntityModel>();

                    break;
                case ObjectType.Unknown:
                case ObjectType.Tag:
                default:
                    throw new NotSupportedException(
                        $"The type '{objectType.GetType()}' is not supported by this method.");
            }

            //TODO: maybe I can use two lists (calculatedTags and tags)
            var query = @$" FOR x in {
                collectionName
            }
                                       FILTER x.Id == ""{
                                           relatedObjectId
                                       }""                                                         
                                             LET newTags = ( x.Tags == null ? {
                                                 tagArray
                                             } :
                                                    MERGE(x.Tags, {
                                                        tagArray
                                                    }) 
                                                           )                
                                              UPDATE x WITH {{Tags: newTags}}
                                            IN {
                                                collectionName
                                            } RETURN NEW";

            MultiApiResponse<object> response = await GetArangoDbClient()
                .ExecuteQueryAsync<object>(query, transactionId, cancellationToken: cancellationToken);

            if (response == null || response.Error)
            {
                Logger.LogWarnMessage(
                    "Error happened by adding tags to the object of type {objectType} with the Id : {Id}",
                    LogHelpers.Arguments(objectType.ToString(), objectId));
            }
        }

        if (IsObjectTypeProfileEntity(objectType))
        {
            await AddTagFromProfileInEntityTree(
                relatedObjectId,
                objectId,
                tagAssignments,
                transaction,
                cancellationToken);

            IList<CalculatedTag> calculatedTags = await CalculateTagsAsync(
                relatedObjectId,
                transactionId,
                cancellationToken);

            await UpdateTagsInProfileAsync(relatedObjectId, calculatedTags, transactionId, cancellationToken);
        }

        //TODO: path and tags calculation
        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task RemoveTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(tagId))
        {
            throw new ArgumentNullException(nameof(tagId));
        }

        DeleteDocumentResponse response =
            await DeleteEntityAsync<Tag>(tagId, transaction, cancellationToken);

        if (response?.Result == null || response.Code == HttpStatusCode.NotFound)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No Tag found with id '{tagId}'.");
        }

        Logger.LogInfoMessage("Deleted tag with id {tagId}.", LogHelpers.Arguments(tagId));

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task RemoveTagFromObjectAsync(
        string relatedProfileId,
        string objectId,
        ObjectType objectType,
        IEnumerable<string> tags,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (relatedProfileId == null)
        {
            throw new ArgumentNullException(nameof(relatedProfileId));
        }

        if (objectId == null)
        {
            throw new ArgumentNullException(nameof(objectId));
        }

        List<string> tagList = tags as List<string> ?? tags?.ToList();

        if (tagList == null || !tagList.Any())
        {
            Logger.LogWarnMessage(
                "Can't remove tags to object: {objectId}, the tag list was leer or empty",
                LogHelpers.Arguments(objectId));

            return;
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        if (relatedProfileId == objectId)
        {
            // TODO: Tags are ids
            JArray tagArray = JArray.FromObject(tags, _serializer);
            string collectionName;
            string tagsCollectionName = GetCollectionName<Tag>();

            switch (objectType)
            {
                case ObjectType.User:
                case ObjectType.Group:
                case ObjectType.Organization:
                case ObjectType.Profile:
                    collectionName = GetCollectionName<IProfileEntityModel>();

                    break;
                case ObjectType.Function:
                case ObjectType.Role:
                    collectionName = GetCollectionName<FunctionObjectEntityModel>();

                    break;
                case ObjectType.Unknown:
                case ObjectType.Tag:
                default:
                    throw new NotSupportedException(
                        $"The type '{objectType.GetType()}' is not supported by this method.");
            }

            //TODO: should be careful tag vs. calculatedTag (update the query)
            // Adapt query for tags (only ids)
            var query = @$"
                               WITH {
                                   tagsCollectionName
                               }, {
                                   collectionName
                               }
                               LET tagList = (
                                              FOR tag IN {
                                                  tagsCollectionName
                                              }
                                              FILTER  tag.Id IN {
                                                  tagArray
                                              }
                                              RETURN tag.{
                                                  nameof(CalculatedTag.Id)
                                              }
                                             ) 
                               FOR profile IN {
                                   collectionName
                               }
                               FILTER profile.{
                                   nameof(IProfileEntityModel.Id)
                               } == ""{
                                   relatedProfileId
                               }""
                               LET newTags = (
                                               profile.{
                                                   nameof(IProfileEntityModel.Tags)
                                               } == null
                                                   ? profile.{
                                                       nameof(IProfileEntityModel.Tags)
                                                   }
                                                   : profile.{
                                                       nameof(IProfileEntityModel.Tags)
                                                   }
                                                        [* FILTER tagList ANY != CURRENT.{
                                                            nameof(CalculatedTag.Id)
                                                        } 
                                                                  AND !CURRENT.{
                                                                      nameof(CalculatedTag.IsInherited)
                                                                  } ]
                                             )
                               UPDATE profile WITH {{ {
                                   nameof(IProfileEntityModel.Tags)
                               }: newTags }}
                                            IN {
                                                collectionName
                                            } RETURN NEW.{
                                                nameof(IProfileEntityModel.Tags)
                                            }";

            IReadOnlyList<string> modifiedProfiles = await ExecuteAqlQueryAsync<string>(
                new ParameterizedAql
                {
                    Query = query
                },
                transactionId,
                true,
                true,
                cancellationToken);

            if (modifiedProfiles == null
                || modifiedProfiles.Count == 0)
            {
                Logger.LogWarnMessage(
                    "Error happened by adding tags to the object of type {objectType} with the Id : {Id}",
                    LogHelpers.Arguments(objectType.ToString(), objectId));
            }
        }

        if (IsObjectTypeProfileEntity(objectType))
        {
            await RemoveTagFromProfileInEntityTree(
                relatedProfileId,
                objectId,
                tagList,
                transaction,
                cancellationToken);

            await RecalculateTagsOfProfile(relatedProfileId, transactionId, cancellationToken);
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task AddMemberOfAsync(
        string relatedProfileId,
        string memberId,
        IList<RangeCondition> conditions,
        ISecondLevelProjectionContainer container,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (relatedProfileId == null)
        {
            throw new ArgumentNullException(nameof(relatedProfileId));
        }

        if (memberId == null)
        {
            throw new ArgumentNullException(nameof(memberId));
        }

        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        if (conditions == null)
        {
            throw new ArgumentNullException(nameof(conditions));
        }

        if (memberId == container.Id)
        {
            throw new ArgumentException("Invalid arguments: container.Id should not be equal to memberId!");
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        IReadOnlyList<SecondLevelProjectionProfileEdgeData> createdEdgeResponse;

        var edgeData = new SecondLevelProjectionProfileEdgeData
        {
            RelatedProfileId = relatedProfileId,
            Conditions = conditions
        };

        if (relatedProfileId == memberId)
        {
            switch (container.ContainerType)
            {
                case ContainerType.Group:
                case ContainerType.Organization:
                    await AddMemberOfToProfileInternalAsync(
                        memberId, // member should be updated, because related profile equals member
                        container,
                        conditions,
                        transactionId,
                        cancellationToken);

                    break;
                case ContainerType.Function:
                case ContainerType.Role:
                    await AddSecurityAssignmentLinkToProfileInternalAsync(
                        memberId, // member should be updated, because related profile equals member
                        container,
                        conditions,
                        transactionId,
                        cancellationToken);

                    break;
                case ContainerType.NotSpecified:
                default:
                    return;
            }

            var targetVertexData = new SecondLevelProjectionProfileVertexData
            {
                RelatedProfileId = relatedProfileId,
                ObjectId = container.Id
            };

            Logger.LogDebugMessage(
                "Create a new vertex representing the container with the id {id} inside the tree graph of the entity {relatedProfileId}",
                LogHelpers.Arguments(container.Id, relatedProfileId));

            await CreateEntityInternalAsync(
                targetVertexData,
                WellKnownSecondLevelProjectionQueries.GetVertexId(targetVertexData),
                false,
                transaction,
                cancellationToken);

            string rootDocId = WellKnownSecondLevelProjectionQueries.GetRootVertexDocumentId(
                _pathTreeVertexCollectionName,
                relatedProfileId);

            string newVertexDocId = WellKnownSecondLevelProjectionQueries.GetDocumentIdOfVertex(
                targetVertexData,
                _pathTreeVertexCollectionName);

            Logger.LogDebugMessage(
                "Create a new edge from document {firstDocId} to {secondDocId} inside the tree graph of the entity {relatedProfileId}",
                LogHelpers.Arguments(rootDocId, newVertexDocId, relatedProfileId));

            createdEdgeResponse = await ExecuteAqlQueryAsync<SecondLevelProjectionProfileEdgeData>(
                WellKnownSecondLevelProjectionQueries
                    .CreateEdgeDataPathTree(
                        rootDocId,
                        newVertexDocId,
                        edgeData,
                        _serializer,
                        _modelsInfo),
                transactionId,
                true,
                false,
                cancellationToken);
        }
        else
        {
            var firstVertexData = new SecondLevelProjectionProfileVertexData
            {
                RelatedProfileId = relatedProfileId,
                ObjectId = memberId
            };

            var secondVertexData = new SecondLevelProjectionProfileVertexData
            {
                RelatedProfileId = relatedProfileId,
                ObjectId = container.Id
            };

            Logger.LogDebugMessage(
                "Create a new vertex representing the member with the id {id} inside the tree graph of the entity {relatedProfileId}",
                LogHelpers.Arguments(memberId, relatedProfileId));

            CreateDocumentResponse createFirstVertexResponse = await CreateEntityInternalAsync(
                firstVertexData,
                WellKnownSecondLevelProjectionQueries.GetVertexId(firstVertexData),
                false,
                transaction,
                cancellationToken);

            if (createFirstVertexResponse == null || createFirstVertexResponse.Error)
            {
                Logger.LogWarnMessage(
                    "Error by creating a new vertex representing the member with the id {id} inside the tree graph of the entity {relatedProfileId}",
                    LogHelpers.Arguments(memberId, relatedProfileId));
            }

            if (createFirstVertexResponse is { Code: HttpStatusCode.Conflict })
            {
                Logger.LogInfoMessage(
                    "The vertex representing the member with the id {id} inside the tree graph of the entity {relatedProfileId} already exists",
                    LogHelpers.Arguments(memberId, relatedProfileId));
            }
            else
            {
                await CheckAResponseAsync(
                    createFirstVertexResponse,
                    true,
                    context: transaction?.CallingService,
                    cancellationToken: cancellationToken);
            }

            Logger.LogDebugMessage(
                "Create a new vertex representing the container with the id {id} inside the tree graph of the entity {relatedProfileId}",
                LogHelpers.Arguments(container.Id, relatedProfileId));

            CreateDocumentResponse createSecondVertexResponse = await CreateEntityInternalAsync(
                secondVertexData,
                WellKnownSecondLevelProjectionQueries.GetVertexId(secondVertexData),
                false,
                transaction,
                cancellationToken);

            if (createSecondVertexResponse == null || createSecondVertexResponse.Error)
            {
                Logger.LogDebugMessage(
                    "Error by creating new vertex representing the container with the id {id} inside the tree graph of the entity {relatedProfileId}",
                    LogHelpers.Arguments(container.Id, relatedProfileId));
            }

            if (createSecondVertexResponse is { Code: HttpStatusCode.Conflict })
            {
                Logger.LogInfoMessage(
                    "The vertex representing the member with the id {id} inside the tree graph of the entity {relatedProfileId} already exists",
                    LogHelpers.Arguments(container.Id, relatedProfileId));
            }
            else
            {
                await CheckAResponseAsync(
                    createSecondVertexResponse,
                    true,
                    context: transaction?.CallingService,
                    cancellationToken: cancellationToken);
            }

            string fromId = WellKnownSecondLevelProjectionQueries.GetDocumentIdOfVertex(
                firstVertexData,
                _pathTreeVertexCollectionName);

            string toId = WellKnownSecondLevelProjectionQueries.GetDocumentIdOfVertex(
                secondVertexData,
                _pathTreeVertexCollectionName);

            Logger.LogDebugMessage(
                "Create a new edge from {fromId} to {toId} inside the tree graph of the entity {relatedProfileId}",
                LogHelpers.Arguments(fromId, toId, relatedProfileId));

            createdEdgeResponse = await ExecuteAqlQueryAsync<SecondLevelProjectionProfileEdgeData>(
                WellKnownSecondLevelProjectionQueries
                    .CreateEdgeDataPathTree(
                        fromId,
                        toId,
                        edgeData,
                        _serializer,
                        _modelsInfo),
                transactionId,
                true,
                true,
                cancellationToken);

            if (createdEdgeResponse == null || createdEdgeResponse.Count == 0)
            {
                Logger.LogWarnMessage(
                    "Error by creating a new edge from {fromId} to {toId} inside the tree graph of the entity {relatedProfileId}",
                    LogHelpers.Arguments(fromId, toId, relatedProfileId));
            }
        }

        if (createdEdgeResponse == null || createdEdgeResponse.Count == 0)
        {
            Logger.LogWarnMessage(
                "Error by adding edge in the collection : {collectionName}",
                LogHelpers.Arguments(_pathTreeEdgeCollectionName));
        }

        await RecalculatePathsAndTagsOfProfileAsync(relatedProfileId, transactionId, cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task RemoveMemberOfAsync(
        string relatedProfileId,
        string memberId,
        ContainerType containerType,
        string containerId,
        IList<RangeCondition> conditions,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (relatedProfileId == null)
        {
            throw new ArgumentNullException(nameof(relatedProfileId));
        }

        if (memberId == null)
        {
            throw new ArgumentNullException(nameof(memberId));
        }

        if (containerId == null)
        {
            throw new ArgumentNullException(nameof(containerId));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        if (relatedProfileId == memberId)
        {
            Logger.LogInfoMessage(
                "Removing container {containerType} {containerId} from query profile {profileId}",
                LogHelpers.Arguments(containerType, containerId, relatedProfileId));

            ParameterizedAql query = containerType switch
            {
                ContainerType.Function => WellKnownSecondLevelProjectionQueries
                    .RemoveSecurityAssignmentEntriesToProfile(
                        relatedProfileId,
                        _modelsInfo,
                        _serializer,
                        containerId,
                        conditions),
                ContainerType.Role => WellKnownSecondLevelProjectionQueries
                    .RemoveSecurityAssignmentEntriesToProfile(
                        relatedProfileId,
                        _modelsInfo,
                        _serializer,
                        containerId,
                        conditions),
                ContainerType.Group => WellKnownSecondLevelProjectionQueries
                    .RemoveMemberOfEntriesToProfile(
                        relatedProfileId,
                        _modelsInfo,
                        _serializer,
                        containerId,
                        conditions),
                ContainerType.Organization => WellKnownSecondLevelProjectionQueries
                    .RemoveMemberOfEntriesToProfile(
                        relatedProfileId,
                        _modelsInfo,
                        _serializer,
                        containerId,
                        conditions),
                _ => throw new NotSupportedException($"Unable to remove {containerType} from profile")
            };

            IReadOnlyList<IProfileEntityModel> modifiedProfiles =
                await ExecuteAqlQueryAsync<IProfileEntityModel>(
                    query,
                    transactionId,
                    true,
                    true,
                    cancellationToken);

            if (!modifiedProfiles.Any())
            {
                throw new InstanceNotFoundException($"Unable to find the profile with id {relatedProfileId}")
                {
                    RelatedId = relatedProfileId
                };
            }
        }

        IReadOnlyList<SecondLevelProjectionProfileEdgeData> edgeObject = null;

        if (conditions != null && conditions.Any())
        {
            ParameterizedAql updatedConditionsQuery = WellKnownSecondLevelProjectionQueries.RemoveConditionsFromEdge(
                relatedProfileId,
                containerId,
                memberId,
                conditions,
                _serializer,
                _modelsInfo);

            edgeObject = await ExecuteAqlQueryAsync<SecondLevelProjectionProfileEdgeData>(
                updatedConditionsQuery,
                transactionId,
                cancellationToken: cancellationToken);
        }

        // if the conditions are empty we should delete the edge
        if (edgeObject != null && !edgeObject[0].Conditions.Any())
        {
            ParameterizedAql deleteEdgeQuery = WellKnownSecondLevelProjectionQueries.GetDeletePathTreeEdgesAql(
                relatedProfileId,
                containerId,
                memberId,
                _modelsInfo);

            IReadOnlyList<string> deletedEdges = await ExecuteAqlQueryAsync<string>(
                deleteEdgeQuery,
                transactionId,
                cancellationToken: cancellationToken);

            if (deletedEdges.Any())
            {
                ParameterizedAql deleteRecursiveGraphOfPathTree =
                    WellKnownSecondLevelProjectionQueries.GetDeleteRecursiveGraphOfPathTreeAql(
                        relatedProfileId,
                        containerId,
                        _modelsInfo);

                await ExecuteAqlQueryAsync<string>(
                    deleteRecursiveGraphOfPathTree,
                    transactionId,
                    cancellationToken: cancellationToken);
            }

            Logger.LogDebugMessage(
                "Recalculating tags and paths of the profile (Id = {profileId}",
                LogHelpers.Arguments(relatedProfileId));
        }

        await RecalculatePathsAndTagsOfProfileAsync(relatedProfileId, transactionId, cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task AddMemberAsync(
        string containerId,
        ContainerType containerType,
        Member member,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (containerId == null)
        {
            throw new ArgumentNullException(nameof(containerId));
        }

        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        if (member.Id == null)
        {
            throw new ArgumentNullException(nameof(member.Id));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        Logger.LogDebugMessage(
            "Adding a new member: {memberId} to the container of type (containerType = {containerType} with the Id (containerId = {containerId})",
            LogHelpers.Arguments(member.Id, containerType.ToString(), containerId));

        JObject newMember = JObject.FromObject(_mapper.Map<AVMember>(member), _serializer);

        switch (containerType)
        {
            case ContainerType.Group:
            case ContainerType.Organization:

                string collectionName = GetCollectionName<IProfileEntityModel>();

                var query = @$"  FOR x in {
                    collectionName
                }
                                        FILTER x.Id == ""{
                                            containerId
                                        }"" 
                                        LET newMember = {
                                            newMember
                                        }
                                        LET currentConditions = FLATTEN(x.Members[* FILTER CURRENT.Id == newMember.Id].Conditions)
                                        LET allConditions = UNION_DISTINCT(currentConditions, newMember.Conditions)
                                        LET newMemberList = APPEND(
                                            x.Members[* FILTER CURRENT.Id != newMember.Id], 
                                            MERGE(newMember, 
                                                {{ 
                                                    Conditions: allConditions,
                                                    IsActive: COUNT(
                                                        FOR c IN allConditions
                                                        FILTER NOT_NULL(c.Start, DATE_ISO8601(DATE_NOW())) <= DATE_ISO8601(DATE_NOW())
                                                        AND NOT_NULL(c.End, DATE_ISO8601(DATE_NOW())) >= DATE_ISO8601(DATE_NOW())
                                                        RETURN c) > 0 
                                                }})
                                        )

                                        UPDATE x WITH 
                                        {{ 
                                            Members: newMemberList, 
                                            HasChildren: COUNT(newMemberList) > 0 
                                        }} IN {
                                            collectionName
                                        } 
                                        RETURN NEW";

                MultiApiResponse<IProfileEntityModel> updateProfileResponse = await GetArangoDbClient()
                    .ExecuteQueryAsync<IProfileEntityModel>(
                        query,
                        transactionId,
                        cancellationToken: cancellationToken);

                if (!updateProfileResponse.QueryResult.Any())
                {
                    throw new InstanceNotFoundException("Unable to find the given entity.")
                    {
                        RelatedId = containerId
                    };
                }

                Logger.LogDebugMessage(
                    "The member: {memberId} has been added to the container of type (containerType = {containerType} with the Id (containerId = {containerId})",
                    LogHelpers.Arguments(member.Id, containerType.ToString(), containerId));

                break;

            case ContainerType.Function:
            case ContainerType.Role:

                collectionName = GetCollectionName<FunctionObjectEntityModel>();

                query = @$" FOR x in {
                    collectionName
                }
                                FILTER x.Id == ""{
                                    containerId
                                }""
                                LET newMember = {
                                    newMember
                                }
                                LET currentConditions = FLATTEN(x.{
                                    nameof(IAssignmentObjectEntity.LinkedProfiles)
                                }[* FILTER CURRENT.Id == newMember.Id].Conditions)
                                LET allConditions = UNION_DISTINCT(currentConditions, newMember.Conditions)
                                LET newMemberList = APPEND(x.{
                                    nameof(IAssignmentObjectEntity.LinkedProfiles)
                                }[* FILTER CURRENT.Id != newMember.Id], MERGE(newMember, {{ Conditions: allConditions }}))
                                UPDATE x WITH {{{
                                    nameof(IAssignmentObjectEntity.LinkedProfiles)
                                }: newMemberList}}
                                  IN {
                                      collectionName
                                  } RETURN NEW";

                MultiApiResponse<IAssignmentObjectEntity> updateFunctionRoleResult = await GetArangoDbClient()
                    .ExecuteQueryAsync<IAssignmentObjectEntity>(
                        query,
                        transactionId,
                        cancellationToken: cancellationToken);

                if (!updateFunctionRoleResult.QueryResult.Any())
                {
                    throw new InstanceNotFoundException("Unable to find the given entity.")
                    {
                        RelatedId = containerId
                    };
                }

                Logger.LogDebugMessage(
                    "The member: {memberId} has been added to the container of type (containerType = {containerType} with the Id (containerId = {containerId})",
                    LogHelpers.Arguments(member.Id, containerType.ToString(), containerId));

                break;
            case ContainerType.NotSpecified:
            default:
                throw new ArgumentException("container type not supported");
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task RemoveMemberAsync(
        string containerId,
        ContainerType containerType,
        string memberId,
        IList<RangeCondition> conditions = null,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (containerId == null)
        {
            throw new ArgumentNullException(nameof(containerId));
        }

        if (memberId == null)
        {
            throw new ArgumentNullException(nameof(memberId));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        switch (containerType)
        {
            case ContainerType.Group:
            case ContainerType.Organization:

                Logger.LogDebugMessage(
                    "Getting the {containerType} corresponding to the Id: {containerId}",
                    LogHelpers.Arguments(containerType.ToString("G"), containerId));

                string collectionName = GetCollectionName<IContainerProfileEntityModel>();

                GetDocumentResponse<IContainerProfileEntityModel> documentResponse = await GetArangoDbClient()
                    .GetDocumentAsync<IContainerProfileEntityModel>(
                        GetDocumentId(collectionName, containerId),
                        transactionId);

                await CheckAResponseAsync(
                        documentResponse,
                        true,
                        context: transaction?.CallingService,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                IContainerProfileEntityModel containerProfile = documentResponse.Result;

                Logger.LogDebugMessage(
                    "Deleting a member: {memberId} from the {containerType} {containerId} ",
                    LogHelpers.Arguments(memberId, containerProfile.Kind.ToString("G"), containerProfile.Id));

                AVMember relatedMember = containerProfile.Members?.FirstOrDefault(g => g.Id == memberId);

                if (relatedMember == null)
                {
                    throw new InstanceNotFoundException(
                        ArangoRepoErrorCodes.MemberNotFoundString,
                        $"Member with the Id: {memberId} could not be found inside the container ({containerProfile.Kind:G}) with the Id: {containerId}");
                }

                if (conditions == null || !conditions.Any())
                {
                    containerProfile.Members.Remove(relatedMember);
                }
                else
                {
                    foreach (RangeCondition rangeCondition in conditions)
                    {
                        AVRangeCondition toDeleteCondition = relatedMember.Conditions?.FirstOrDefault(
                            c => c.Start == rangeCondition.Start && c.End == rangeCondition.End);

                        if (toDeleteCondition != null)
                        {
                            relatedMember.Conditions.Remove(toDeleteCondition);
                        }
                    }

                    relatedMember.IsActive = relatedMember.Conditions?.Any(IsRangeConditionActive) ?? false;

                    if (relatedMember.Conditions == null
                        || !relatedMember.Conditions.Any())
                    {
                        containerProfile.Members.Remove(relatedMember);
                    }
                }

                JArray newMemberList = JArray.FromObject(containerProfile.Members, _serializer);

                var query = @$" FOR x in {
                    collectionName
                }
                    FILTER x.Id == ""{
                        containerId
                    }""                 
                    UPDATE x WITH {{
                        Members: {
                            newMemberList
                        },
                        HasChildren: {
                            newMemberList.Any()
                        }
                    }}
                    IN {
                        collectionName
                    } RETURN NEW";

                await GetArangoDbClient()
                    .ExecuteQueryAsync<IProfileEntityModel>(
                        query,
                        transactionId,
                        cancellationToken: cancellationToken);

                break;
            case ContainerType.Function:
            case ContainerType.Role:

                Logger.LogDebugMessage(
                    "Getting the {containerType} corresponding to the Id: {containerId}",
                    LogHelpers.Arguments(containerType.ToString("G"), containerId));

                collectionName = GetCollectionName<IAssignmentObjectEntity>();

                GetDocumentResponse<IAssignmentObjectEntity> assignmentObjectResponse = await GetArangoDbClient()
                    .GetDocumentAsync<IAssignmentObjectEntity>(
                        GetDocumentId(GetCollectionName<IAssignmentObjectEntity>(), containerId),
                        transactionId);

                await CheckAResponseAsync(
                        assignmentObjectResponse,
                        true,
                        context: transaction?.CallingService,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                IAssignmentObjectEntity assignmentObject = assignmentObjectResponse.Result;

                Logger.LogDebugMessage(
                    "Removing member {memberId} from the {containerType} {containerId} ",
                    LogHelpers.Arguments(memberId, assignmentObject.Type.ToString("G"), assignmentObject.Id));

                AVMember relatedLinkedProfile =
                    assignmentObject.LinkedProfiles?.FirstOrDefault(g => g.Id == memberId);

                if (relatedLinkedProfile == null)
                {
                    throw new InstanceNotFoundException(
                        ArangoRepoErrorCodes.MemberNotFoundString,
                        $"Member (linkedProfile) with the Id: {memberId} could not be found inside the container ({assignmentObject.Type:G}) with the Id: {containerId}");
                }

                if (conditions == null || !conditions.Any())
                {
                    assignmentObject.LinkedProfiles.Remove(relatedLinkedProfile);
                }
                else
                {
                    foreach (RangeCondition rangeCondition in conditions)
                    {
                        AVRangeCondition toDeleteCondition = relatedLinkedProfile.Conditions?.FirstOrDefault(
                            c => c.Start == rangeCondition.Start && c.End == rangeCondition.End);

                        if (toDeleteCondition != null)
                        {
                            relatedLinkedProfile.Conditions.Remove(toDeleteCondition);
                        }
                    }

                    relatedLinkedProfile.IsActive =
                        relatedLinkedProfile.Conditions?.Any(IsRangeConditionActive) ?? false;

                    if (relatedLinkedProfile.Conditions == null || !relatedLinkedProfile.Conditions.Any())
                    {
                        assignmentObject.LinkedProfiles.Remove(relatedLinkedProfile);
                    }
                }

                newMemberList = JArray.FromObject(assignmentObject.LinkedProfiles, _serializer);

                query = @$" FOR x in {
                    collectionName
                }
                    FILTER x.Id == ""{
                        containerId
                    }""                 
                    UPDATE x WITH {{LinkedProfiles: {
                        newMemberList
                    }}}
                    IN {
                        collectionName
                    }";

                await GetArangoDbClient()
                    .ExecuteQueryAsync<IAssignmentObjectEntity>(
                        query,
                        transactionId,
                        cancellationToken: cancellationToken);

                break;
            case ContainerType.NotSpecified:
            default:
                throw new ArgumentException("container type not supported");
        }
    }

    /// <inheritdoc />
    public async Task UpdateProfileAsync(
        ISecondLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (profile.Id == null)
        {
            throw new ArgumentNullException(nameof(profile.Id));
        }

        string profileId = profile.Id;
        string collectionName = GetCollectionName<IProfileEntityModel>();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        if (transactionId == null)
        {
            Logger.LogDebugMessage(
                "Updating profile: {profileId} in the collection: {collectionName}.",
                LogHelpers.Arguments(profileId, collectionName));
        }
        else
        {
            Logger.LogDebugMessage(
                "Updating profile: {profileId} in the collection:{collectionName} inside the transaction: {transactionId}.",
                LogHelpers.Arguments(profileId, collectionName, transactionId));
        }

        UpdateDocumentResponse<JToken> response = await GetArangoDbClient()
            .UpdateDocumentAsync(
                GetDocumentId(collectionName, profileId),
                profile.GetJsonDocument(new ReadOnlyPropertyJsonConverter()),
                transactionId: transactionId);

        await CheckAResponseAsync(
                response,
                true,
                context: transaction?.CallingService,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task UpdateFunctionAsync(
        SecondLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (function.Id == null)
        {
            throw new ArgumentNullException(nameof(function.Id));
        }

        string functionId = function.Id;
        string collectionName = GetCollectionName<FunctionObjectEntityModel>();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        GetDocumentResponse<FunctionObjectEntityModel> functionCurrentEntity = await GetArangoDbClient()
            .GetDocumentAsync<FunctionObjectEntityModel>($"{collectionName}/{function.Id}", transactionId);

        if (transactionId == null)
        {
            Logger.LogDebugMessage(
                "Updating function: {functionId} in the collection: {collectionName}.",
                LogHelpers.Arguments(functionId, collectionName));
        }
        else
        {
            Logger.LogDebugMessage(
                "Updating function: {functionId} in the collection:{collectionName} inside the transaction: {transactionId}.",
                LogHelpers.Arguments(functionId, collectionName, transactionId));
        }

        FunctionObjectEntityModel updatedFunctionEntity =
            functionCurrentEntity.Result.UpdateFunctionModel(function, _mapper);

        UpdateDocumentResponse<JToken> response = await GetArangoDbClient()
            .UpdateDocumentAsync(
                GetDocumentId(collectionName, functionId),
                updatedFunctionEntity.GetJsonDocument(),
                transactionId: transactionId);

        await CheckAResponseAsync(
                response,
                true,
                context: transaction?.CallingService,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task UpdateRoleAsync(
        SecondLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (role.Id == null)
        {
            throw new ArgumentNullException(nameof(role.Id));
        }

        string roleId = role.Id;
        string collectionName = GetCollectionName<RoleObjectEntityModel>();
        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        if (transactionId == null)
        {
            Logger.LogDebugMessage(
                "Updating role: {roleId} in the collection: {collectionName}.",
                LogHelpers.Arguments(roleId, collectionName));
        }
        else
        {
            Logger.LogDebugMessage(
                "Updating role: {roleId} in the collection:{collectionName} inside the transaction: {transactionId}.",
                LogHelpers.Arguments(roleId, collectionName, transactionId));
        }

        UpdateDocumentResponse<JToken> response = await GetArangoDbClient()
            .UpdateDocumentAsync(
                GetDocumentId(collectionName, roleId),
                role.GetJsonDocument(new ReadOnlyPropertyJsonConverter()),
                transactionId: transactionId);

        await CheckAResponseAsync(
                response,
                true,
                context: transaction?.CallingService,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task TryUpdateMemberAsync(
        string relatedProfileId,
        string memberIdentifier,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (changedPropertySet == null || !changedPropertySet.Any())
        {
            Logger.LogDebugMessage(
                "Nothing updated, the parameter {changedPropertySet} was null or empty",
                LogHelpers.Arguments(nameof(changedPropertySet)));

            return;
        }

        try
        {
            string collectionName = GetCollectionName<IProfileEntityModel>();
            IEnumerable<string> validProperties = typeof(AVMember).GetProperties().Select(p => p.Name);

            Dictionary<string, object> checkedChangedPropertySet = changedPropertySet
                .Where(entry => validProperties.Contains(entry.Key))
                .ToDictionary(
                    item => item.Key,
                    item => item.Value);

            if (!checkedChangedPropertySet.Any())
            {
                Logger.LogInfoMessage(
                    "No properties to update for the profile {relatedProfileId}",
                    LogHelpers.Arguments(relatedProfileId));

                return;
            }

            string transactionId = ValidateTransaction(transaction)?.TransactionId;
            JObject changes = JObject.FromObject(checkedChangedPropertySet, _serializer);

            var query = @$" FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        relatedProfileId
                    }""
                    LET alteredList = (
                        FOR element IN x.Members
                    LET newItem = (element.Id == ""{
                        memberIdentifier
                    }"" ?
                        MERGE(element, {
                            changes
                        }) : element
                       )
                    RETURN newItem)
                    UPDATE x WITH {{Members: alteredList}}
                    IN {
                        collectionName
                    }";

            MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
                .ExecuteQueryAsync<IProfileEntityModel>(
                    query,
                    transactionId,
                    cancellationToken: cancellationToken);

            if (response == null || response.Error)
            {
                Logger.LogWarnMessage(
                    "Error during updating member with {id}, inside the container profile {relatedProfileId}",
                    LogHelpers.Arguments(memberIdentifier, relatedProfileId));
            }

            Logger.ExitMethod(response);
        }
        catch (Exception)
        {
            Logger.LogWarnMessage(
                "The member with the {Id} could not be updated",
                LogHelpers.Arguments(memberIdentifier));

            Logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task TryUpdateMemberOfAsync(
        string relatedProfileId,
        string memberIdentifier,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (changedPropertySet == null || !changedPropertySet.Any())
        {
            Logger.LogDebugMessage(
                "Nothing updated, the parameter {changedPropertySet} was null or empty",
                LogHelpers.Arguments(nameof(changedPropertySet)));

            return;
        }

        try
        {
            string collectionName = GetCollectionName<IProfileEntityModel>();
            IEnumerable<string> validProperties = typeof(AVMember).GetProperties().Select(p => p.Name);

            Dictionary<string, object> checkedChangedPropertySet = changedPropertySet
                .Where(entry => validProperties.Contains(entry.Key))
                .ToDictionary(
                    item => item.Key,
                    item => item.Value);

            if (!checkedChangedPropertySet.Any())
            {
                Logger.LogInfoMessage(
                    "No properties to update for the profile {relatedProfileId}",
                    LogHelpers.Arguments(relatedProfileId));

                return;
            }

            string transactionId = ValidateTransaction(transaction)?.TransactionId;
            JObject changes = JObject.FromObject(checkedChangedPropertySet, _serializer);

            var query = @$" FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        relatedProfileId
                    }""
                    LET alteredList = (
                        FOR element IN x.MemberOf
                    LET newItem = (element.Id == ""{
                        memberIdentifier
                    }"" ?
                        MERGE(element, {
                            changes
                        }) : element
                       )
                    RETURN newItem)
                    UPDATE x WITH {{MemberOf: alteredList}}
                    IN {
                        collectionName
                    }";

            MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
                .ExecuteQueryAsync<IProfileEntityModel>(
                    query,
                    transactionId,
                    cancellationToken: cancellationToken);

            if (response == null || response.Error)
            {
                Logger.LogWarnMessage(
                    "Error during updating memberOf with {id}, inside the container profile {relatedProfileId}",
                    LogHelpers.Arguments(memberIdentifier, relatedProfileId));
            }
        }
        catch (Exception)
        {
            Logger.LogWarnMessage(
                "The memberOf with the {Id} could not be updated",
                LogHelpers.Arguments(memberIdentifier));
        }
    }

    /// <inheritdoc />
    public async Task TryUpdateLinkedObjectAsync(
        string relatedProfileId,
        string linkedObjectId,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (changedPropertySet == null || !changedPropertySet.Any())
        {
            Logger.LogDebugMessage(
                "Nothing updated, the parameter {changedPropertySet} was null or empty",
                LogHelpers.Arguments(nameof(changedPropertySet)));

            return;
        }

        try
        {
            string collectionName = GetCollectionName<IProfileEntityModel>();
            IEnumerable<string> validProperties = typeof(ILinkedObject).GetProperties().Select(p => p.Name);

            Dictionary<string, object> checkedChangedPropertySet = changedPropertySet
                .Where(entry => validProperties.Contains(entry.Key))
                .ToDictionary(
                    item => item.Key,
                    item => item.Value);

            if (!checkedChangedPropertySet.Any())
            {
                Logger.LogInfoMessage(
                    "No properties to update for the profile {relatedProfileId}",
                    LogHelpers.Arguments(relatedProfileId));

                return;
            }

            string transactionId = ValidateTransaction(transaction)?.TransactionId;
            JObject changes = JObject.FromObject(checkedChangedPropertySet, _serializer);

            var query = @$"FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        relatedProfileId
                    }""
                    LET alteredList = (
                        FOR element IN x.SecurityAssignments
                    LET newItem = (element.Id == ""{
                        linkedObjectId
                    }"" ?
                        MERGE(element, {
                            changes
                        }) : element
                       )
                    RETURN newItem)
                    UPDATE x WITH {{SecurityAssignments: alteredList}}
                    IN {
                        collectionName
                    }";

            MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
                .ExecuteQueryAsync<IProfileEntityModel>(
                    query,
                    transactionId,
                    cancellationToken: cancellationToken);

            if (response == null || response.Error)
            {
                Logger.LogWarnMessage(
                    "Error during updating linked object with {id}, inside the profile {relatedProfileId}",
                    LogHelpers.Arguments(linkedObjectId, relatedProfileId));

                Logger.ExitMethod(response);

                return;
            }

            Logger.LogInfoMessage(
                "Linked object with the Id {id} has been updated inside the profile {profileId}",
                LogHelpers.Arguments(linkedObjectId, relatedProfileId));

            Logger.ExitMethod(response);
        }
        catch (Exception)
        {
            Logger.LogWarnMessage(
                "The linked object with the Id: {linkedObjectId} could not be updated on the profile {relatedProfileId}",
                LogHelpers.Arguments(relatedProfileId));

            Logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task TryUpdateLinkedProfileAsync(
        string relatedLinkedObjectId,
        string linkedProfileId,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (changedPropertySet == null || !changedPropertySet.Any())
        {
            Logger.LogDebugMessage(
                "Nothing updated, the parameter {changedPropertySet} was null or empty",
                LogHelpers.Arguments(nameof(changedPropertySet)));

            return;
        }

        try
        {
            string collectionName = GetCollectionName<FunctionObjectEntityModel>();
            IEnumerable<string> validProperties = typeof(AVMember).GetProperties().Select(p => p.Name);

            Dictionary<string, object> checkedChangedPropertySet = changedPropertySet
                .Where(entry => validProperties.Contains(entry.Key))
                .ToDictionary(
                    item => item.Key,
                    item => item.Value);

            if (!checkedChangedPropertySet.Any())
            {
                Logger.LogInfoMessage(
                    "No properties to update for the function or role with the Id {relatedProfileId}",
                    LogHelpers.Arguments(relatedLinkedObjectId));

                return;
            }

            string transactionId = ValidateTransaction(transaction)?.TransactionId;
            JObject changes = JObject.FromObject(checkedChangedPropertySet, _serializer);

            var query = @$" FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        relatedLinkedObjectId
                    }""
                    LET alteredList = (
                        FOR element IN x.LinkedProfiles
                    LET newItem = (element.Id == ""{
                        linkedProfileId
                    }"" ?
                        MERGE(element, {
                            changes
                        }) : element
                       )
                    RETURN newItem)
                    UPDATE x WITH {{LinkedProfiles: alteredList}}
                    IN {
                        collectionName
                    }";

            MultiApiResponse<IProfileEntityModel> response = await GetArangoDbClient()
                .ExecuteQueryAsync<IProfileEntityModel>(
                    query,
                    transactionId,
                    cancellationToken: cancellationToken);

            if (response == null || response.Error)
            {
                Logger.LogWarnMessage(
                    $"Error during updating the linked object {{id}}, inside the function or role with the Id: {relatedLinkedObjectId}",
                    LogHelpers.Arguments(linkedProfileId, relatedLinkedObjectId));

                Logger.ExitMethod(response);

                return;
            }

            Logger.LogInfoMessage(
                "Linked object with the Id {id} has been updated inside the function or role {id}",
                LogHelpers.Arguments(linkedProfileId, relatedLinkedObjectId));

            Logger.ExitMethod(response);
        }
        catch (Exception)
        {
            Logger.LogWarnMessage(
                "The linked profile with the Id: {linkedProfileId} could not be updated on the role or function with the id {relatedLinkedObjectId}",
                LogHelpers.Arguments(linkedProfileId, relatedLinkedObjectId));

            Logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task RecalculateAssignmentsAsync(
        ObjectIdent relatedEntity,
        string profileId,
        string targetId,
        ObjectType targetType,
        bool assignmentIsActive,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (relatedEntity == null)
        {
            throw new ArgumentNullException(nameof(relatedEntity));
        }

        if (profileId == null)
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        if (targetId == null)
        {
            throw new ArgumentNullException(nameof(targetId));
        }

        if (string.IsNullOrWhiteSpace(relatedEntity.Id))
        {
            throw new ArgumentException("relatedEntity.Id cannot be null or whitespace.", nameof(relatedEntity));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("profileId cannot be null or whitespace.", nameof(profileId));
        }

        if (string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("targetId cannot be null or whitespace.", nameof(targetId));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        cancellationToken.ThrowIfCancellationRequested();

        if (!relatedEntity.Type.IsProfileType())
        {
            if (relatedEntity.Type != Maverick.UserProfileService.Models.EnumModels.ObjectType.Function
                && relatedEntity.Type != Maverick.UserProfileService.Models.EnumModels.ObjectType.Role)
            {
                return;
            }

            Logger.LogDebugMessage(
                "Recalculating LinkedProfiles for (role/function) {containerId} Member {profileId}",
                Arguments(relatedEntity.Id, profileId));

            await ExecuteAqlQueryAsync<object>(
                WellKnownSecondLevelProjectionQueries.RecalculateLinkedProfilesForProfilesForRolesAndFunctions(
                    relatedEntity.Id,
                    _modelsInfo,
                    profileId),
                transactionId,
                cancellationToken: cancellationToken);

            return;
        }

        // calculate related profile, because it's tree has to be changed
        await RecalculatePathsAndTagsOfProfileAsync(
            relatedEntity.Id,
            transactionId,
            cancellationToken);

        if (relatedEntity.Id == targetId)
        {
            // members has changed
            Logger.LogDebugMessage(
                "Recalculating Member for profile {profileId} Member {memberId}",
                Arguments(relatedEntity.Id, profileId));

            await ExecuteAqlQueryAsync<object>(
                WellKnownSecondLevelProjectionQueries.RecalculateMemberEntriesForProfile(
                    relatedEntity.Id,
                    _modelsInfo,
                    profileId),
                transactionId,
                cancellationToken: cancellationToken);
        }
        else if (relatedEntity.Id == profileId)
        {
            // MemberOf or SecurityAssignment has changed
            if (targetType.IsProfileType())
            {
                // memberOf has changed
                Logger.LogDebugMessage(
                    "Recalculating MemberOf for profile {profileId} container (profile) {containerId}",
                    Arguments(relatedEntity.Id, targetId));

                await ExecuteAqlQueryAsync<object>(
                    WellKnownSecondLevelProjectionQueries.RecalculateMemberOfEntriesToProfile(
                        relatedEntity.Id,
                        _modelsInfo,
                        targetId),
                    transactionId,
                    cancellationToken: cancellationToken);
            }
            else
            {
                // SecurityAssignment has changed
                Logger.LogDebugMessage(
                    "Recalculating MemberOf for profile {profileId} container (role/function) {containerId}",
                    Arguments(relatedEntity.Id, targetId));

                await ExecuteAqlQueryAsync<object>(
                    WellKnownSecondLevelProjectionQueries.RecalculateSecurityAssignmentEntriesToProfile(
                        relatedEntity.Id,
                        _modelsInfo,
                        targetId),
                    transactionId,
                    cancellationToken: cancellationToken);
            }
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task DeleteProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentNullException(nameof(profileId));
        }

        string transactionId = ValidateTransaction(transaction)?.TransactionId;

        DeleteDocumentResponse response =
            await DeleteEntityAsync<IProfileEntityModel>(profileId, transaction, cancellationToken);

        if (response?.Result == null || response.Code == HttpStatusCode.NotFound)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.ProfileNotFoundString,
                $"No profile found with id '{profileId}'.");
        }

        Logger.LogInfoMessage("Deleted profile with id {profileId}.", LogHelpers.Arguments(profileId));

        await DeleteEntityInPathTree(profileId, transactionId, cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task DeleteFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        DeleteDocumentResponse response =
            await DeleteEntityAsync<FunctionObjectEntityModel>(functionId, transaction, cancellationToken);

        if (response?.Result == null || response.Code == HttpStatusCode.NotFound)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.RoleOrFunctionNotFoundString,
                $"No function found with id '{functionId}'.");
        }

        Logger.LogInfoMessage("Deleted function with id {functionId}.", LogHelpers.Arguments(functionId));
        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task DeleteRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        DeleteDocumentResponse response =
            await DeleteEntityAsync<RoleObjectEntityModel>(roleId, transaction, cancellationToken);

        if (response?.Result == null || response.Code == HttpStatusCode.NotFound)
        {
            throw new InstanceNotFoundException(
                ArangoRepoErrorCodes.RoleOrFunctionNotFoundString,
                $"No role found with id '{roleId}'.");
        }

        Logger.LogInfoMessage("Deleted role with id {roleId}.", LogHelpers.Arguments(roleId));
        Logger.ExitMethod();
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
