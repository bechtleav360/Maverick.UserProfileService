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
///     <see cref="ISynchronizationWriteDestination{FunctionSync}" />
/// </summary>
public class MaverickFunctionDestinationSystem : BaseMaverickDestinationSystem<MaverickFunctionDestinationSystem>,
    ISynchronizationReadDestination<FunctionSync>,
    ISynchronizationWriteDestination<FunctionSync>
{
    private readonly IMapper _mapper;
    private readonly IProfileService _profileService;

    /// <summary>
    ///     Create an instance of <see cref="MaverickFunctionDestinationSystem" />
    /// </summary>
    /// <param name="profileService">The service that provides all operations to get related objects.</param>
    /// <param name="mapper">Mapper for objects.</param>
    /// <param name="messageBus">Broker to publish messages.</param>
    /// <param name="logger">The logger.</param>
    public MaverickFunctionDestinationSystem(
        IProfileService profileService,
        IMapper mapper,
        IBus messageBus,
        ILogger<MaverickFunctionDestinationSystem> logger) : base(messageBus, logger)
    {
        _profileService = profileService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<FunctionSync> GetObjectAsync(string id, CancellationToken token)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage("Try to get function for id {id}", LogHelpers.Arguments(id));

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        try
        {
            FunctionSync functionSync = await _profileService.GetFunctionAsync(id, token);

            Logger.LogInfoMessage(
                "Found function for id {id} and name {name} ",
                LogHelpers.Arguments(id, functionSync.Name));

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTraceMessage(
                    "Found function for id {id} with data: {data}",
                    LogHelpers.Arguments(id, JsonConvert.SerializeObject(functionSync)));
            }

            return Logger.ExitMethod(functionSync);
        }
        catch (InstanceNotFoundException)
        {
            Logger.LogInfoMessage("Function for id {id} could not be found.", LogHelpers.Arguments(id));

            return Logger.ExitMethod((FunctionSync)null);
        }
    }

    /// <inheritdoc />
    public async Task<ICollection<FunctionSync>> GetObjectsAsync(
        KeyProperties externalObjectId,
        CancellationToken token)
    {
        Logger.EnterMethod();

        if (externalObjectId.Filter == null)
        {
            throw new ArgumentException("External object id is invalid.", nameof(externalObjectId));
        }

        var queryObject = new AssignmentQueryObject
        {
            Filter = externalObjectId.Filter
        };

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Try to get function for query: {query} ",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        IPaginatedList<FunctionSync> paginatedResult =
            await _profileService.GetFunctionsAsync<FunctionSync>(queryObject, token);

        paginatedResult = externalObjectId.ExecutePostFilter(paginatedResult);

        if (paginatedResult.TotalAmount == 0)
        {
            Logger.LogInfoMessage("Could not found any function for query.", LogHelpers.Arguments());

            return Logger.ExitMethod<IList<FunctionSync>>(null);
        }

        Logger.LogInfoMessage(
            "Found {total} functions for page {page} with limit {limit} and offset {offset}.",
            LogHelpers.Arguments(paginatedResult.TotalAmount, queryObject.Limit, queryObject.Offset));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Found {total} functions with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    paginatedResult.TotalAmount,
                    queryObject.Limit,
                    queryObject.Offset,
                    string.Join(" , ", paginatedResult.Select(p => p.Id))));
        }

        List<FunctionSync> functions = paginatedResult.ToList();

        return Logger.ExitMethod(functions);
    }

    /// <inheritdoc />
    public async Task<IBatchResult<FunctionSync>> GetObjectsAsync(
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
                "Try to get function for query: {query} ",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        IPaginatedList<FunctionSync> paginatedResult =
            await _profileService.GetFunctionsAsync<FunctionSync>(
                queryObject,
                token);

        int currentPosition = start + batchSize;
        bool nextBatch = paginatedResult.TotalAmount > currentPosition;

        paginatedResult = externalObjectId.ExecutePostFilter(paginatedResult);

        Logger.LogInfoMessage(
            "Found {total} functions with limit {limit} and offset {offset}.",
            LogHelpers.Arguments(paginatedResult.TotalAmount, queryObject.Limit, queryObject.Offset));

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTraceMessage(
                "Found {total} functions with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    paginatedResult.TotalAmount,
                    queryObject.Limit,
                    queryObject.Offset,
                    string.Join(" , ", paginatedResult.Select(p => p.Id))));
        }

        List<FunctionSync> functions = paginatedResult.ToList();
        var batchResult = new BatchResult<FunctionSync>(functions, start, currentPosition, batchSize, nextBatch);

        return Logger.ExitMethod(batchResult);
    }

    /// <inheritdoc />
    public Task<CommandResult> CreateObjectAsync(
        Guid collectingId,
        FunctionSync sourceObject,
        CancellationToken token)
    {
        Logger.EnterMethod();

        throw new NotSupportedException("Creating functions is not supported yet.");
    }

    /// <inheritdoc />
    public Task<CommandResult> UpdateObjectAsync(
        Guid collectingId,
        FunctionSync sourceObject,
        ISet<string> modifiedProperties,
        CancellationToken token)
    {
        Logger.EnterMethod();

        throw new NotSupportedException("Updating functions is not supported yet.");
    }

    /// <inheritdoc />
    public async Task<IList<CommandResult>> DeleteObjectsAsync(
        Guid collectingId,
        IList<FunctionSync> objectsToDelete,
        string sourceSystem,
        CancellationToken token)
    {
        Logger.EnterMethod();

        var results = new List<CommandResult>();

        Logger.LogInfoMessage(
            "Try to delete {count} function in maverick system.",
            LogHelpers.Arguments(objectsToDelete.Count));

        foreach (FunctionSync @object in objectsToDelete)
        {
            Logger.LogDebugMessage(
                "Try to delete function with id {id} in maverick system.",
                LogHelpers.Arguments(@object.Id));

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTraceMessage(
                    "Try to delete function with id {id} in maverick system. Function Data: {data}",
                    LogHelpers.Arguments(@object.Id, JsonConvert.SerializeObject(@object)));
            }

            try
            {
                FunctionDeletedMessage payloadMessage = _mapper.Map<FunctionSync, FunctionDeletedMessage>(@object);

                CommandResult result = await ExecuteAsync(
                    collectingId,
                    payloadMessage,
                    token);

                Logger.LogDebugMessage(
                    "Delete function with id {id} in maverick system with result id {id}",
                    LogHelpers.Arguments(@object.Id, result.Id));

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTraceMessage(
                        "Delete function with id {id} in maverick system with result: {result}",
                        LogHelpers.Arguments(@object.Id, JsonConvert.SerializeObject(result)));
                }

                results.Add(result);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "An error occurred while publishing message to delete object with maverick id '{objectId}' and external id '{externalId}'.",
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
    public Task<IList<RelationProcessingObject>> HandleRelationsAsync(
        Guid collectingId,
        IList<IRelation> relations,
        bool delete = false,
        CancellationToken token = default)
    {
        Logger.EnterMethod();

        // nothing to do here

        Task<IList<RelationProcessingObject>> result =
            Task.FromResult<IList<RelationProcessingObject>>(new List<RelationProcessingObject>());

        return Logger.ExitMethod(result);
    }
}
