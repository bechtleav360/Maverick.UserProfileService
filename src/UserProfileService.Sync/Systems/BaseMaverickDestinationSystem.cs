using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Saga.Events.Extensions;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Systems;

/// <summary>
///     The class hold all necessary methods to synchronize all objects in the maverick destination system.
/// </summary>
/// <typeparam name="TMaverickDestinationSystem">The type of the logger.</typeparam>
public abstract class BaseMaverickDestinationSystem<TMaverickDestinationSystem>
{
    /// <summary>
    ///     A logger for logging purposes.
    /// </summary>
    protected readonly ILogger<TMaverickDestinationSystem> Logger;

    /// <summary>
    ///     Component that communicates with the configured message broker.
    /// </summary>
    protected readonly IBus MessageBus;

    /// <summary>
    ///     Creates an instance of <see cref="BaseMaverickDestinationSystem{TMaverickDestinationSystem}" />.
    /// </summary>
    /// <param name="messageBus">Component that communicates with the configured message broker.</param>
    /// <param name="logger">A logger for logging purposes.</param>
    protected BaseMaverickDestinationSystem(
        IBus messageBus,
        ILogger<TMaverickDestinationSystem> logger)
    {
        MessageBus = messageBus;
        Logger = logger;
    }

    private void HandleAssignmentMessage(
        Dictionary<Type, List<RelationProcessingObject>> relationMessages,
        ILookUpObject resource,
        bool assigned,
        AssignmentType assignmentType,
        params ObjectRelation[] profileIds)
    {
        Logger.EnterMethod();

        ConditionObjectIdent[] assignments =
            profileIds.Select(x => new ConditionObjectIdent(x.MaverickId, x.ObjectType)).ToArray();

        ConditionObjectIdent[] added = assigned ? assignments : Array.Empty<ConditionObjectIdent>();
        ConditionObjectIdent[] removed = !assigned ? assignments : Array.Empty<ConditionObjectIdent>();

        HandleAssignmentMessage(relationMessages, resource, added, removed, assignmentType);

        Logger.ExitMethod();
    }

    private void HandleAssignmentMessage(
        Dictionary<Type, List<RelationProcessingObject>> relationMessages,
        ILookUpObject resource,
        ConditionObjectIdent[] added,
        ConditionObjectIdent[] removed,
        AssignmentType assignmentType)
    {
        Logger.EnterMethod();

        bool messageExists =
            relationMessages.TryGetValue(
                typeof(ObjectAssignmentMessage),
                out List<RelationProcessingObject> messages);

        if (!messageExists)
        {
            messages = new List<RelationProcessingObject>();
            relationMessages.Add(typeof(ObjectAssignmentMessage), messages);
        }

        RelationProcessingObject processingObject = messages
            .FirstOrDefault(
                m =>
                {
                    var assignmentMessage = m.GetMessage<ObjectAssignmentMessage>();

                    return assignmentMessage.Resource.Id == resource.MaverickId
                        && assignmentMessage.Type == assignmentType;
                });

        if (processingObject == null)
        {
            processingObject = new RelationProcessingObject<ObjectAssignmentMessage>
            {
                Message = new ObjectAssignmentMessage
                {
                    Resource = new ObjectIdent(
                        resource.MaverickId,
                        resource.ObjectType),
                    Type = assignmentType,
                    Added = Array.Empty<ConditionObjectIdent>(),
                    Removed = Array.Empty<ConditionObjectIdent>()
                },
                Relation = new Relation(
                    new LookUpObject(
                        resource.ExternalId,
                        resource.MaverickId,
                        resource.Source,
                        resource.ObjectType))
            };

            messages.Add(processingObject);
        }

        IEnumerable<ObjectRelation> newAddRelatedObjects = added.Select(
            pi =>
                new ObjectRelation(assignmentType, null, pi.Id, pi.Type));

        IEnumerable<ObjectRelation> newRemovedRelatedObjects = removed.Select(
            pi =>
                new ObjectRelation(assignmentType, null, pi.Id, pi.Type));

        processingObject.Relation.RelatedObjects.AddRange(newAddRelatedObjects);
        processingObject.Relation.RelatedObjects.AddRange(newRemovedRelatedObjects);

        var message = processingObject.GetMessage<ObjectAssignmentMessage>();
        message.Added = message.Added.Concat(added).ToArray();
        message.Removed = message.Removed.Concat(removed).ToArray();

        Logger.ExitMethod();
    }

    private void HandleFunctionPropertiesChangedMessage(
        Dictionary<Type, List<RelationProcessingObject>> relationMessages,
        ILookUpObject function,
        string relatedRoleId)
    {
        throw new NotImplementedException();
    }

    private protected async Task<RelationProcessingObject> ExecuteAsync(
        Guid collectingId,
        RelationProcessingObject processingObject,
        SynchronizationOperation operation,
        CancellationToken token)
    {
        Logger.EnterMethod();

        // workaround, because a separation must be made between update and delete.
        // If this separation is not used,
        // the system tries to find the correct state based on the type (IRelation),
        // which is wrong at this point.
        switch (operation)
        {
            case SynchronizationOperation.Add:
            case SynchronizationOperation.Delete:
            {
                processingObject.Result = await ExecuteAsync(
                    collectingId,
                    processingObject.Message,
                    token);

                break;
            }
            case SynchronizationOperation.Nothing:
            case SynchronizationOperation.Update:
            case SynchronizationOperation.All:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Only add and update operation are specified for handling relations.");
        }

        return Logger.ExitMethod(processingObject);
    }

    private protected async Task<CommandResult> ExecuteAsync<TMessage>(
        Guid collectingId,
        TMessage message,
        CancellationToken token)
    {
        Logger.EnterMethod();

        var commandId = Guid.NewGuid();

        try
        {
            SubmitCommand commandMessage = message.ToCommand(
                commandId.ToString(),
                collectingId,
                new CommandInitiator(SyncConstants.System.InitiatorId, CommandInitiatorType.System));

            await MessageBus.Publish(
                commandMessage,
                token);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error occurred while sending message of type {type} to message broker.",
                LogHelpers.Arguments(typeof(TMessage).Name));

            return new CommandResult(commandId, e);
        }

        return Logger.ExitMethod(new CommandResult(commandId));
    }

    private protected async Task<IList<RelationProcessingObject>> InternalHandleRelationsAsync(
        Guid collectingId,
        IList<IRelation> relations,
        bool deleteRelations = false,
        CancellationToken token = default)
    {
        Logger.EnterMethod();

        var relationMessages = new Dictionary<Type, List<RelationProcessingObject>>();

        foreach (IRelation relation in relations)
        {
            foreach (ObjectRelation relatedObject in relation.RelatedObjects)
            {
                if (relatedObject.AssignmentType != AssignmentType.Unknown)
                {
                    if ((relation.OriginalObject.ObjectType.IsProfileType()
                            && relatedObject.ObjectType.IsContainerProfileType())
                        || (relation.OriginalObject.ObjectType.IsContainerProfileType()
                            && relatedObject.ObjectType.IsProfileType()))
                    {
                        HandleAssignmentMessage(
                            relationMessages,
                            relation.OriginalObject,
                            !deleteRelations,
                            relatedObject.AssignmentType,
                            relatedObject);
                    }
                    else
                    {
                        Logger.LogErrorMessage(
                            null,
                            "Related object with id '{relatedObject.MaverickId}' and type '{relatedObject.ObjectType}' cannot be set as parent of object with id '{relation.OriginalObject.MaverickId}' and type '{relation.OriginalObject.ObjectType}'.",
                            LogHelpers.Arguments(
                                relatedObject.MaverickId,
                                relatedObject.ObjectType,
                                relation.OriginalObject.MaverickId,
                                relation.OriginalObject.ObjectType));
                    }
                }

                if (relatedObject.AssignmentType == AssignmentType.Unknown)
                {
                    if (relation.OriginalObject.ObjectType.IsProfileType()
                        && relatedObject.ObjectType.IsProfileType())
                    {
                        Logger.LogErrorMessage(
                            null,
                            "Relation object '{relation.OriginalObject.MaverickId}' and related object '{relatedObject.MaverickId}' cannot be assigned, because both are profiles.",
                            LogHelpers.Arguments(relation.OriginalObject.MaverickId, relatedObject.MaverickId));

                        continue;
                    }

                    if ((!relation.OriginalObject.ObjectType.IsProfileType()
                            && relatedObject.ObjectType.IsProfileType())
                        || (relation.OriginalObject.ObjectType.IsProfileType()
                            && !relation.OriginalObject.ObjectType.IsProfileType()))
                    {
                        HandleAssignmentMessage(
                            relationMessages,
                            relation.OriginalObject,
                            !deleteRelations,
                            AssignmentType.Unknown,
                            relatedObject);
                    }

                    if (deleteRelations)
                    {
                        continue;
                    }

                    if (relation.OriginalObject.ObjectType == ObjectType.Role
                        && relatedObject.ObjectType == ObjectType.Function)
                    {
                        HandleFunctionPropertiesChangedMessage(
                            relationMessages,
                            relatedObject,
                            relation.OriginalObject.MaverickId);
                    }

                    if (relation.OriginalObject.ObjectType == ObjectType.Function
                        && relatedObject.ObjectType == ObjectType.Role)
                    {
                        HandleFunctionPropertiesChangedMessage(
                            relationMessages,
                            relation.OriginalObject,
                            relatedObject.MaverickId);
                    }
                }
            }
        }

        IEnumerable<RelationProcessingObject> messages = relationMessages
            .SelectMany(s => s.Value)
            .Select(
                s =>
                {
                    if (deleteRelations)
                    {
                        s.Relation = new DeletedRelation(s.Relation);
                    }
                    else
                    {
                        s.Relation = new AddedRelation(s.Relation);
                    }

                    return s;
                });

        SynchronizationOperation operation =
            deleteRelations ? SynchronizationOperation.Delete : SynchronizationOperation.Add;

        IEnumerable<Task<RelationProcessingObject>> messageResultsTask = messages
            .Select(
                message =>
                    ExecuteAsync(
                        collectingId,
                        message,
                        operation,
                        token));

        RelationProcessingObject[] messageResults = await Task.WhenAll(messageResultsTask);

        return Logger.ExitMethod(messageResults.ToList());
    }
}
