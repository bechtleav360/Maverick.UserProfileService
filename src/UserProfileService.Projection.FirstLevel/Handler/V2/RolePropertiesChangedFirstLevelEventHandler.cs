using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Utilities;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="RolePropertiesChangedEvent" />.
/// </summary>
internal class RolePropertiesChangedFirstLevelEventHandler : FirstLevelEventHandlerBase<RolePropertiesChangedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly IMapper _Mapper;
    private readonly IPropertiesChangedRelatedEventsResolver _PropertiesChangedRelatedEventsResolver;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="RolePropertiesChangedFirstLevelEventHandler" />.
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
    /// <param name="propertiesChangedRelatedEventsResolver"></param>
    public RolePropertiesChangedFirstLevelEventHandler(
        ILogger<RolePropertiesChangedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper,
        IPropertiesChangedRelatedEventsResolver propertiesChangedRelatedEventsResolver) :
        base(logger, repository)
    {
        _SagaService = sagaService;
        _Creator = creator;
        _Mapper = mapper;
        _PropertiesChangedRelatedEventsResolver = propertiesChangedRelatedEventsResolver;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        RolePropertiesChangedEvent eventObject,
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

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "@event: {event}.",
                LogHelpers.Arguments(eventObject.ToLogString()));
        }

        FirstLevelProjectionRole role = await Repository.GetRoleAsync(
            eventObject.Payload.Id,
            transaction,
            cancellationToken);

        var propertiesChangedEvent = _Mapper.Map<PropertiesChanged>(eventObject);
        propertiesChangedEvent.RelatedContext = PropertiesChangedContext.Self;

        Guid batchSagaId = await _SagaService.CreateBatchAsync(
            cancellationToken,
            _Creator.CreateEvents(
                    role.ToObjectIdent(),
                    new List<IUserProfileServiceEvent>
                    {
                        propertiesChangedEvent
                    },
                    eventObject)
                .ToArray());

        IList<ObjectIdentPath> relatedObjects =
            await ExecuteSafelyAsync(
                () => Repository.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    role.ToObjectIdent(),
                    transaction,
                    cancellationToken),
                new List<ObjectIdentPath>());

        role.UpdateRoleWithPayload(eventObject.Payload, eventObject, Logger);

        if (relatedObjects.Any())
        {
            (List<ObjectIdentPath> related, List<ObjectIdentPath> functions) objectIdents =
                RolePropertiesChangedHelper.SplitRelevantObjectsToSendEvents(relatedObjects, role.Id, Logger);

            foreach (ObjectIdentPath function in objectIdents.functions)
            {
                FunctionPropertiesChangedEvent functionCreatedEvent =
                    eventObject.Payload.CreateFunctionEventRelatedToPropertiesPayload(eventObject);

                List<EventTuple> functionEvents =
                    await _PropertiesChangedRelatedEventsResolver.CreateFunctionPropertiesChangedEventsAsync(
                        function.Id,
                        functionCreatedEvent,
                        PropertiesChangedContext.Role,
                        transaction,
                        cancellationToken);

                await _SagaService.AddEventsAsync(batchSagaId, functionEvents, cancellationToken);
            }

            // Are properties changed that affects other entity? And are some other 
            // entities tied to the role other than then the function?
            if (eventObject.Payload.MembersHasToBeUpdated() && objectIdents.related.Any())
            {
                List<EventTuple> membersChanged = RolePropertiesChangedHelper.HandleMembersChangedThroughRole(
                    objectIdents.related.ToList(),
                    _Mapper.Map<Role>(role),
                    eventObject,
                    _Creator);

                await _SagaService.AddEventsAsync(batchSagaId, membersChanged, cancellationToken);
            }
        }

        await Repository.UpdateRoleAsync(role, transaction, cancellationToken);
        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
