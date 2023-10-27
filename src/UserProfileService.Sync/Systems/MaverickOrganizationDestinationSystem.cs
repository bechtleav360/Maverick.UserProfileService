using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Models.Results;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Systems;

/// <summary>
///     Maverick implementation of <see cref="ISynchronizationReadDestination{T}" /> and
///     <see cref="ISynchronizationWriteDestination{OrganizationSync}" />
/// </summary>
public class MaverickOrganizationDestinationSystem :
    BaseMaverickDestinationSystem<MaverickOrganizationDestinationSystem>,
    ISynchronizationReadDestination<OrganizationSync>,
    ISynchronizationWriteDestination<OrganizationSync>
{
    private readonly IMapper _mapper;
    private readonly IProfileService _profileService;

    /// <summary>
    ///     Create an instance of <see cref="MaverickOrganizationDestinationSystem" />
    /// </summary>
    /// <param name="profileService">The service that provides all operations to get related objects.</param>
    /// <param name="mapper">Mapper for objects.</param>
    /// <param name="messageBus">Broker to publish messages.</param>
    /// <param name="logger">The logger.</param>
    public MaverickOrganizationDestinationSystem(
        IProfileService profileService,
        IMapper mapper,
        IBus messageBus,
        ILogger<MaverickOrganizationDestinationSystem> logger) : base(
        messageBus,
        logger)
    {
        _profileService = profileService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<OrganizationSync> GetObjectAsync(string id, CancellationToken token)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        try
        {
            Logger.LogInfoMessage("Try to get organization for id {id}", LogHelpers.Arguments(id));

            var organizationSync =
                await _profileService.GetProfileAsync<OrganizationSync>(
                    id,
                    token);

            Logger.LogInfoMessage(
                "Found organization for id {id} and name {name}.",
                LogHelpers.Arguments(id, organizationSync.Name));

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTraceMessage(
                    "Found organization for id {id} with data: {data}",
                    LogHelpers.Arguments(id, JsonConvert.SerializeObject(organizationSync)));
            }

            return Logger.ExitMethod(organizationSync);
        }
        catch (InstanceNotFoundException)
        {
            Logger.LogInfoMessage("Organization for id {id} could not be found.", LogHelpers.Arguments(id));

            return Logger.ExitMethod((OrganizationSync)null);
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<OrganizationSync>> GetObjectsAsync(
        KeyProperties externalObjectId,
        CancellationToken token)
    {
        Logger.EnterMethod();

        if (externalObjectId?.Filter == null)
        {
            throw new ArgumentException("External object id is invalid", nameof(externalObjectId));
        }

        var queryObject = new AssignmentQueryObject
        {
            Filter = externalObjectId.Filter
        };

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Try to get organization for query: {query} ",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        IPaginatedList<OrganizationSync> paginatedResult =
            await _profileService.GetOrganizationsAsync(
                queryObject,
                token);

        paginatedResult = externalObjectId.ExecutePostFilter(paginatedResult);

        if (paginatedResult.TotalAmount == 0)
        {
            Logger.LogInfoMessage("Could not found any organization for query.", LogHelpers.Arguments());

            return Logger.ExitMethod<ICollection<OrganizationSync>>(null);
        }

        Logger.LogInfoMessage(
            "Found {total} organizations with limit {limit} and offset {offset}.",
            LogHelpers.Arguments(paginatedResult.TotalAmount, queryObject.Limit, queryObject.Offset));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Found {total} organizations with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    paginatedResult.TotalAmount,
                    queryObject.Limit,
                    queryObject.Offset,
                    string.Join(" , ", paginatedResult.Select(p => p.Id))));
        }

        List<OrganizationSync> organizations = paginatedResult.ToList();

        return Logger.ExitMethod(organizations);
    }

    /// <inheritdoc />
    public async Task<IBatchResult<OrganizationSync>> GetObjectsAsync(
        int start,
        int batchSize,
        DateTime stamp,
        CancellationToken token,
        KeyProperties externalObjectId = null)
    {
        Logger.EnterMethod();

        var queryObject = new AssignmentQueryObject
        {
            Filter = externalObjectId?.Filter,
            Limit = batchSize,
            Offset = start
        };

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Try to get organization for query: {query} ",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        IPaginatedList<OrganizationSync> paginatedResult =
            await _profileService.GetOrganizationsAsync(
                queryObject,
                token);

        int currentPosition = start + batchSize;
        bool nextBatch = paginatedResult.TotalAmount > currentPosition;

        paginatedResult = externalObjectId.ExecutePostFilter(paginatedResult);

        Logger.LogInfoMessage(
            "Found {total} organizations with limit {limit} and offset {offset}.",
            LogHelpers.Arguments(paginatedResult.TotalAmount, queryObject.Limit, queryObject.Offset));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Found {total} organizations with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    paginatedResult.TotalAmount,
                    queryObject.Limit,
                    queryObject.Offset,
                    string.Join(" , ", paginatedResult.Select(p => p.Id))));
        }

        List<OrganizationSync> organizations = paginatedResult.Where(p => p.Kind == ProfileKind.Organization).ToList();

        var batchResult = new BatchResult<OrganizationSync>(
            organizations,
            start,
            currentPosition,
            batchSize,
            nextBatch);

        return Logger.ExitMethod(batchResult);
    }

    /// <inheritdoc />
    public async Task<CommandResult> CreateObjectAsync(
        Guid collectingId,
        OrganizationSync sourceObject,
        CancellationToken token)
    {
        Logger.EnterMethod();
        Logger.LogInfoMessage("Create organization with id {id}.", LogHelpers.Arguments(sourceObject.Id));

        OrganizationCreatedMessage payloadMessage =
            _mapper.Map<OrganizationSync, OrganizationCreatedMessage>(sourceObject);

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Create organization with id {id} and data: {organization}.",
                LogHelpers.Arguments(sourceObject.Id, JsonConvert.SerializeObject(payloadMessage)));
        }

        CommandResult result = await ExecuteAsync(
            collectingId,
            payloadMessage,
            token);

        Logger.LogDebugMessage(
            "Create organization with id {id} in maverick system with result id {id}",
            LogHelpers.Arguments(sourceObject.Id, result.Id));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Create organization with id {id} in maverick system with result: {result}",
                LogHelpers.Arguments(sourceObject.Id, JsonConvert.SerializeObject(result)));
        }

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<CommandResult> UpdateObjectAsync(
        Guid collectingId,
        OrganizationSync sourceObject,
        ISet<string> modifiedProperties,
        CancellationToken token)
    {
        Logger.EnterMethod();

        string joinedModifiedProperties = string.Join(" , ", modifiedProperties);

        Logger.LogInfoMessage(
            "Update organization with id {id} and modified properties {properties}.",
            LogHelpers.Arguments(sourceObject.Id, joinedModifiedProperties));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Update organization with id {id}, modified properties {properties} and source data: {organization}.",
                LogHelpers.Arguments(
                    sourceObject.Id,
                    joinedModifiedProperties,
                    JsonConvert.SerializeObject(sourceObject)));
        }

        IDictionary<string, object> modifiedPropertyValues =
            sourceObject.ExtractProperties(modifiedProperties, Logger);

        var profilePropertiesChangedMessage = new ProfilePropertiesChangedMessage
        {
            Id = sourceObject.Id,
            ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind.Organization,
            Properties = modifiedPropertyValues
        };

        CommandResult result = await ExecuteAsync(
            collectingId,
            profilePropertiesChangedMessage,
            token);

        Logger.LogDebugMessage(
            "Update organization with id {id} in maverick system with result id {id}",
            LogHelpers.Arguments(sourceObject.Id, result.Id));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Update organization with id {id} in maverick system with result: {result}",
                LogHelpers.Arguments(sourceObject.Id, JsonConvert.SerializeObject(result)));
        }

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IList<CommandResult>> DeleteObjectsAsync(
        Guid collectingId,
        IList<OrganizationSync> objectsToDelete,
        string sourceSystem,
        CancellationToken token)
    {
        Logger.EnterMethod();

        var results = new List<CommandResult>();

        Logger.LogInfoMessage(
            "Try to delete {count} organization in maverick system.",
            LogHelpers.Arguments(objectsToDelete.Count));

        foreach (OrganizationSync @object in objectsToDelete)
        {
            Logger.LogDebugMessage(
                "Try to delete organization with id {id} in maverick system.",
                LogHelpers.Arguments(@object.Id));

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTraceMessage(
                    "Try to delete organization with id {id} in maverick system. Organization data: {data}",
                    LogHelpers.Arguments(@object.Id, JsonConvert.SerializeObject(@object)));
            }

            try
            {
                ProfileDeletedMessage payloadMessage =
                    _mapper.Map<OrganizationSync, ProfileDeletedMessage>(@object);

                CommandResult result = await ExecuteAsync(
                    collectingId,
                    payloadMessage,
                    token);

                Logger.LogDebugMessage(
                    "Delete organization with id {id} in maverick system with result id {id}",
                    LogHelpers.Arguments(@object.Id, result.Id));

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTraceMessage(
                        "Delete organization with id {id} in maverick system with result: {result}",
                        LogHelpers.Arguments(@object.Id, JsonConvert.SerializeObject(result)));
                }

                results.Add(result);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "An error occurred while publishing message to delete object with maverick id '{id}' and external id '{externalId}'.",
                    LogHelpers.Arguments(
                        @object.Id,
                        @object.ExternalIds.FirstOrDefaultUnconverted(sourceSystem)?.Id));

                var result = new CommandResult(Guid.Empty, e);

                results.Add(result);
            }
        }

        return Logger.ExitMethod(results);
    }

    /// <inheritdoc />
    public async Task<IList<RelationProcessingObject>> HandleRelationsAsync(
        Guid collectingId,
        IList<IRelation> relations,
        bool delete = false,
        CancellationToken token = default)
    {
        Logger.EnterMethod();

        IList<RelationProcessingObject> relationResults =
            await InternalHandleRelationsAsync(
                collectingId,
                relations,
                delete,
                token);

        return Logger.ExitMethod(relationResults);
    }
}
