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
using RoleCreatedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.RoleCreated;
using ResolvedInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using TagAssignmentResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.TagAssignment;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="RoleCreatedEvent" />.
/// </summary>
internal class RoleCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<RoleCreatedEvent>
{
    /// <summary>
    ///     Creates an instance of the object <see cref="RoleCreatedFirstLevelEventHandler" />.
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
    public RoleCreatedFirstLevelEventHandler(
        ILogger<RoleCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(
        logger,
        repository,
        sagaService,
        mapper,
        creator)
    {
    }

    protected override async Task HandleInternalAsync(
        RoleCreatedEvent eventObject,
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

        var firstLevelRole = Mapper.Map<FirstLevelProjectionRole>(eventObject);

        await Repository.CreateRoleAsync(firstLevelRole, transaction, cancellationToken);

        var roleCreatedResolvedEvent = Mapper.Map<RoleCreatedResolvedEvent>(eventObject);

        Guid batchSagaId = await SagaService.CreateBatchAsync(
            cancellationToken,
            Creator.CreateEvents(
                    firstLevelRole.ToObjectIdent(),
                    new List<IUserProfileServiceEvent>
                    {
                        roleCreatedResolvedEvent
                    },
                    eventObject)
                .ToArray());

        await CreateTagsAddedEvent(
            transaction,
            batchSagaId,
            eventObject.Payload.Tags.ToList(),
            async (repo, tag) =>
                await repo.AddTagToRoleAsync(
                    tag,
                    eventObject.Payload.Id,
                    transaction,
                    cancellationToken),
            eventObject,
            firstLevelRole.ToObjectIdent(),
            cancellationToken);

        await SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
