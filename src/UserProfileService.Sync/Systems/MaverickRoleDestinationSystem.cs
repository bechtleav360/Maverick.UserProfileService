using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
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
using QueryObject = Maverick.UserProfileService.Models.RequestModels.QueryObject;

namespace UserProfileService.Sync.Systems;

/// <summary>
///     Maverick implementation of <see cref="ISynchronizationReadDestination{T}" /> and
///     <see cref="ISynchronizationWriteDestination{RoleSync}" />
/// </summary>
public class MaverickRoleDestinationSystem : BaseMaverickDestinationSystem<MaverickRoleDestinationSystem>,
    ISynchronizationReadDestination<RoleSync>,
    ISynchronizationWriteDestination<RoleSync>
{
    private readonly IMapper _mapper;
    private readonly IProfileService _profileService;

    /// <summary>
    ///     Create an instance of <see cref="MaverickRoleDestinationSystem" />
    /// </summary>
    /// <param name="profileService">The service that provides all operations to get related objects.</param>
    /// <param name="mapper">Mapper for objects.</param>
    /// <param name="messageBus">Broker to publish messages.</param>
    /// <param name="logger">The logger.</param>
    public MaverickRoleDestinationSystem(
        IProfileService profileService,
        IMapper mapper,
        IBus messageBus,
        ILogger<MaverickRoleDestinationSystem> logger) : base(
        messageBus,
        logger)
    {
        _profileService = profileService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<RoleSync> GetObjectAsync(string id, CancellationToken token)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        try
        {
            Logger.LogInfoMessage("Try to get role for id {id}", LogHelpers.Arguments(id));

            RoleSync roleSync = await _profileService.GetRoleAsync(id, token);

            Logger.LogInfoMessage(
                "Found role for id {id} and name {name} ",
                LogHelpers.Arguments(id, roleSync.Name));

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTraceMessage(
                    "Found role for id {id} with data: {data}.",
                    LogHelpers.Arguments(id, JsonConvert.SerializeObject(roleSync)));
            }

            return Logger.ExitMethod(roleSync);
        }
        catch (InstanceNotFoundException)
        {
            Logger.LogInfoMessage("Role for id {id} could not be found.", LogHelpers.Arguments(id));

            return Logger.ExitMethod((RoleSync)null);
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<RoleSync>> GetObjectsAsync(
        KeyProperties externalObjectId,
        CancellationToken token)
    {
        Logger.EnterMethod();

        if (externalObjectId.Filter == null)
        {
            throw new ArgumentException("External object id is invalid", nameof(externalObjectId));
        }

        var queryObject = new QueryObject
        {
            Filter = externalObjectId.Filter
        };

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Try to get role for query: {query}.",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        IPaginatedList<RoleSync> paginatedResult =
            await _profileService.GetRolesAsync<RoleSync>(queryObject, token);

        paginatedResult = externalObjectId.ExecutePostFilter(paginatedResult);

        if (paginatedResult.TotalAmount == 0)
        {
            Logger.LogInfoMessage("Could not found any role for query.", LogHelpers.Arguments());

            return Logger.ExitMethod<ICollection<RoleSync>>(null);
        }

        List<RoleSync> roles = paginatedResult.ToList();

        Logger.LogInfoMessage(
            "Found {total} roles with limit {limit} and offset {offset}.",
            LogHelpers.Arguments(paginatedResult.TotalAmount, queryObject.Limit, queryObject.Offset));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogInfoMessage(
                "Found {total} roles with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    paginatedResult.TotalAmount,
                    queryObject.Limit,
                    queryObject.Offset,
                    string.Join(" , ", paginatedResult.Select(p => p.Id))));
        }

        return Logger.ExitMethod(roles);
    }

    /// <inheritdoc />
    public async Task<IBatchResult<RoleSync>> GetObjectsAsync(
        int start,
        int batchSize,
        DateTime stamp,
        CancellationToken token,
        KeyProperties externalObjectId = null)
    {
        Logger.EnterMethod();

        var queryObject = new QueryObject
        {
            Filter = externalObjectId?.Filter,
            Limit = batchSize,
            Offset = start
        };

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Try to get role for query: {query} ",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        IPaginatedList<RoleSync> paginatedResult =
            await _profileService.GetRolesAsync<RoleSync>(
                queryObject,
                token);

        int currentPosition = start + batchSize;
        bool nextBatch = paginatedResult.TotalAmount > currentPosition;

        paginatedResult = externalObjectId.ExecutePostFilter(paginatedResult);

        Logger.LogInfoMessage(
            "Found {total} roles with limit {limit} and offset {offset}.",
            LogHelpers.Arguments(paginatedResult.TotalAmount, queryObject.Limit, queryObject.Offset));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Found {total} roles with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    paginatedResult.TotalAmount,
                    queryObject.Limit,
                    queryObject.Offset,
                    string.Join(" , ", paginatedResult.Select(p => p.Id))));
        }

        List<RoleSync> roles = paginatedResult.ToList();
        var batchResult = new BatchResult<RoleSync>(roles, start, currentPosition, batchSize, nextBatch);

        return Logger.ExitMethod(batchResult);
    }

    /// <inheritdoc />
    public async Task<CommandResult> CreateObjectAsync(
        Guid collectingId,
        RoleSync sourceObject,
        CancellationToken token)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage("Create role with id {id}.", LogHelpers.Arguments(sourceObject.Id));

        RoleCreatedMessage payloadMessage = _mapper.Map<RoleSync, RoleCreatedMessage>(sourceObject);

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Create role with id {id} and data: {role}.",
                LogHelpers.Arguments(sourceObject.Id, JsonConvert.SerializeObject(payloadMessage)));
        }

        CommandResult result = await ExecuteAsync(
            collectingId,
            payloadMessage,
            token);

        Logger.LogDebugMessage(
            "Create role with id {id} in maverick system with result id {id}",
            LogHelpers.Arguments(sourceObject.Id, result.Id));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Create role with id {id} in maverick system with result: {result}",
                LogHelpers.Arguments(sourceObject.Id, JsonConvert.SerializeObject(result)));
        }

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<CommandResult> UpdateObjectAsync(
        Guid collectingId,
        RoleSync sourceObject,
        ISet<string> modifiedProperties,
        CancellationToken token)
    {
        Logger.EnterMethod();

        string joinedModifiedProperties = string.Join(" , ", modifiedProperties);

        Logger.LogInfoMessage(
            "Update role with id {id} and modified properties {properties}.",
            LogHelpers.Arguments(sourceObject.Id, joinedModifiedProperties));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Update role with id {id}, modified properties {properties} and source data: {role}.",
                LogHelpers.Arguments(
                    sourceObject.Id,
                    joinedModifiedProperties,
                    JsonConvert.SerializeObject(sourceObject)));
        }

        IDictionary<string, object> modifiedPropertyValues = sourceObject.ExtractProperties(modifiedProperties);

        var profilePropertiesChangedMessage = new RolePropertiesChangedMessage
        {
            Id = sourceObject.Id,
            Properties = modifiedPropertyValues
        };

        CommandResult result = await ExecuteAsync(
            collectingId,
            profilePropertiesChangedMessage,
            token);

        Logger.LogDebugMessage(
            "Update role with id {id} in maverick system with result id {id}.",
            LogHelpers.Arguments(sourceObject.Id, result.Id));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Update role with id {id} in maverick system with result: {result}",
                LogHelpers.Arguments(sourceObject.Id, JsonConvert.SerializeObject(result)));
        }

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<IList<CommandResult>> DeleteObjectsAsync(
        Guid collectingId,
        IList<RoleSync> objectsToDelete,
        string sourceSystem,
        CancellationToken token)
    {
        Logger.EnterMethod();

        var results = new List<CommandResult>();

        Logger.LogInfoMessage(
            "Try to delete {count} role in maverick system.",
            LogHelpers.Arguments(objectsToDelete.Count));

        foreach (RoleSync @object in objectsToDelete)
        {
            Logger.LogDebugMessage(
                "Try to delete role with id {id} in maverick system.",
                LogHelpers.Arguments(@object.Id));

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTraceMessage(
                    "Try to delete role with id {id} in maverick system. Role data: {data}",
                    LogHelpers.Arguments(@object.Id, JsonConvert.SerializeObject(@object)));
            }

            try
            {
                RoleDeletedMessage payloadMessage = _mapper.Map<RoleSync, RoleDeletedMessage>(@object);

                CommandResult result = await ExecuteAsync(
                    collectingId,
                    payloadMessage,
                    token);

                Logger.LogDebugMessage(
                    "Delete role with id {id} in maverick system with result id {id}",
                    LogHelpers.Arguments(@object.Id, result.Id));

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTraceMessage(
                        "Delete role with id {id} in maverick system with result: {result}",
                        LogHelpers.Arguments(@object.Id, JsonConvert.SerializeObject(result)));
                }

                results.Add(result);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "An error occurred while publishing message to delete object with maverick id '{objectId}' and external id '{externalId}'",
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
