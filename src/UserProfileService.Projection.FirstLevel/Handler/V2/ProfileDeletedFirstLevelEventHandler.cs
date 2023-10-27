using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
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
using EntityDeletedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.EntityDeleted;
using MemberDeletedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.MemberDeleted;
using ContainerDeletedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.ContainerDeleted;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="ProfileDeletedEvent" />.
/// </summary>
internal class ProfileDeletedFirstLevelEventHandler : FirstLevelEventHandlerBase<ProfileDeletedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly IMapper _Mapper;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="ProfileDeletedFirstLevelEventHandler" />.
    /// </summary>
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
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    public ProfileDeletedFirstLevelEventHandler(
        ILogger<ProfileDeletedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(logger, repository)
    {
        _SagaService = sagaService;
        _Creator = creator;
        _Mapper = mapper;
    }

    protected override async Task HandleInternalAsync(
        ProfileDeletedEvent eventObject,
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

        IFirstLevelProjectionProfile profileToDelete = await Repository.GetProfileAsync(
            eventObject.Payload.Id,
            transaction,
            cancellationToken);

        var entityDeletedResolvedEvent = new EntityDeletedResolvedEvent
        {
            Id = profileToDelete.Id,
            EventId = eventObject.EventId
        };

        Guid batchSagaId = await _SagaService.CreateBatchAsync(
            cancellationToken,
            _Creator.CreateEvents(
                    profileToDelete.ToObjectIdent(),
                    new List<IUserProfileServiceEvent>
                    {
                        entityDeletedResolvedEvent
                    },
                    eventObject)
                .ToArray());

        IList<FirstLevelRelationProfile> relatedChildren = await ExecuteSafelyAsync(
            () => Repository.GetAllChildrenAsync(
                profileToDelete.ToObjectIdent(),
                transaction,
                cancellationToken),
            new List<FirstLevelRelationProfile>());

        IList<IFirstLevelProjectionContainer> relatedParents =
            await ExecuteSafelyAsync(
                () => Repository.GetParentsAsync(profileToDelete.Id, transaction, cancellationToken),
                new List<IFirstLevelProjectionContainer>());

        if (relatedChildren.Any())
        {
            foreach (FirstLevelRelationProfile relation in relatedChildren)
            {
                var containerDeletedResolvedEvent = _Mapper.Map<ContainerDeletedResolvedEvent>(eventObject);
                containerDeletedResolvedEvent.MemberId = relation.Profile.Id;

                await _SagaService.AddEventsAsync(
                    batchSagaId,
                    _Creator.CreateEvents(
                        relation.Profile.ToObjectIdent(),
                        new List<IUserProfileServiceEvent>
                        {
                            containerDeletedResolvedEvent
                        },
                        eventObject),
                    cancellationToken);
            }
        }

        if (relatedParents.Any())
        {
            foreach (IFirstLevelProjectionContainer parent in relatedParents)
            {
                var memberDeletedResolvedEvent = new MemberDeletedResolvedEvent
                {
                    ContainerId = parent.Id,
                    MemberId = eventObject.Payload.Id
                };

                await _SagaService.AddEventsAsync(
                    batchSagaId,
                    _Creator.CreateEvents(
                        parent.ToObjectIdent(),
                        new List<IUserProfileServiceEvent>
                        {
                            memberDeletedResolvedEvent
                        },
                        eventObject),
                    cancellationToken);
            }
        }

        await Repository.DeleteProfileAsync(eventObject.Payload.Id, transaction, cancellationToken);
        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
