using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="RoleDeletedEvent" />.
/// </summary>
internal class RoleDeletedFirstLevelEventHandler : FirstLevelEventHandlerBase<RoleDeletedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="RoleDeletedFirstLevelEventHandler" />.
    /// </summary>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    /// <param name="logger">
    ///     The logger factory that is used to create a logger. The logger logs message for debugging
    ///     and control reasons.
    /// </param>
    /// <param name="repository">
    ///     The read service is used to read from the internal query storage to get all information to
    ///     generate all needed stream events.
    /// </param>
    /// <param name="sagaService">
    ///     The saga service is used to write all created <see cref="IUserProfileServiceEvent" /> to the
    ///     write stream.
    /// </param>
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    public RoleDeletedFirstLevelEventHandler(
        ILogger<RoleDeletedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(logger, repository)
    {
        _SagaService = sagaService;
        _Creator = creator;
    }

    protected override async Task HandleInternalAsync(
        RoleDeletedEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (streamEvent == null)
        {
            throw new ArgumentNullException(nameof(streamEvent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "@event: {{event}}, streamEventHeader: {streamEvent}",
                LogHelpers.Arguments(eventObject.ToLogString(), streamEvent.ToLogString()));
        }

        FirstLevelProjectionRole role = await Repository.GetRoleAsync(
            eventObject.Payload.Id,
            transaction,
            cancellationToken);

        Guid batchSagaId = await _SagaService.CreateBatchAsync(cancellationToken);

        IList<ObjectIdentPath> relevantObjects = await ExecuteSafelyAsync(
            () => Repository.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                role.ToObjectIdent(),
                transaction,
                cancellationToken),
            new List<ObjectIdentPath>());

        // all entities that have to be deleted + the role itself
        List<ObjectIdent> entitiesToBeDeleted = relevantObjects.Select(obj => new ObjectIdent(obj.Id, obj.Type))
            .Where(relObj => relObj.Type == ObjectType.Function)
            .Append(
                new ObjectIdent(
                    eventObject.Payload.Id,
                    ObjectType.Role))
            .ToList();

        // create entity deleted for role that has been the deleted and the functions that are
        // affected by the role deletion.
        List<EventTuple> deletedEntitiesTuples = entitiesToBeDeleted.Select(
                t => _Creator.CreateEvent(
                    t,
                    new EntityDeleted
                    {
                        Id = t.Id
                    },
                    eventObject))
            .ToList();

        List<ObjectIdent> functionsToBeDeleted =
            entitiesToBeDeleted.Where(entity => entity.Type != ObjectType.Role).ToList();

        var containerDeletedEventTuple = new List<EventTuple>();

        foreach (ObjectIdent entityToDelete in entitiesToBeDeleted)
        {
            // Get the members of the functions or the role
            IList<ObjectIdent> members = await ExecuteSafelyAsync(
                () => Repository.GetContainerMembersAsync(
                    entityToDelete,
                    transaction,
                    cancellationToken),
                new List<ObjectIdent>());

            // create a container deleted event for the children of the member
            foreach (ObjectIdent member in members)
            {
                if (member.Type != ObjectType.User)
                {
                    List<ObjectIdent> childrenOfMember = (await ExecuteSafelyAsync(
                            () => Repository.GetAllChildrenAsync(
                                member,
                                transaction,
                                cancellationToken),
                            new List<FirstLevelRelationProfile>()))
                        .Select(relation => relation.Profile.ToObjectIdent())
                        .ToList();

                    containerDeletedEventTuple.AddRange(
                        childrenOfMember.Select(
                            child => _Creator.CreateEvent(
                                child,
                                new ContainerDeleted
                                {
                                    ContainerId = entityToDelete.Id,
                                    ContainerType = entityToDelete.Type.ToContainerType(),
                                    MemberId = member.Id
                                },
                                eventObject)));
                }

                // the member itself has to be informed as well
                containerDeletedEventTuple.Add(
                    _Creator.CreateEvent(
                        member,
                        new ContainerDeleted
                        {
                            ContainerId = entityToDelete.Id,
                            ContainerType = entityToDelete.Type.ToContainerType(),
                            MemberId = member.Id
                        },
                        eventObject));
            }
        }

        // delete role in repository
        await Repository.DeleteRoleAsync(eventObject.Payload.Id, transaction, cancellationToken);

        // delete affected functions (affected by role)
        if (functionsToBeDeleted.Any())
        {
            await Task.WhenAll(
                entitiesToBeDeleted
                    .Select(
                        entity => Repository.DeleteFunctionAsync(
                            entity.Id,
                            transaction,
                            cancellationToken)));
        }

        await _SagaService.AddEventsAsync(
            batchSagaId,
            deletedEntitiesTuples.Concat(containerDeletedEventTuple),
            cancellationToken);

        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
