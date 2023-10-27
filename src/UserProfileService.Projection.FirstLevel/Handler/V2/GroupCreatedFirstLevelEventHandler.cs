using System;
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
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="OrganizationCreatedEvent" />.
/// </summary>
internal class GroupCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<GroupCreatedEvent>
{
    /// <summary>
    ///     Creates an instance of the object <see cref="GroupCreatedFirstLevelEventHandler" />.
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
    public GroupCreatedFirstLevelEventHandler(
        ILogger<GroupCreatedEvent> logger,
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
        GroupCreatedEvent eventObject,
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

        var firstLevelProjectionGroup = Mapper.Map<FirstLevelProjectionGroup>(eventObject);
        var createdGroup = Mapper.Map<GroupCreated>(eventObject);

        await Repository.CreateProfileAsync(firstLevelProjectionGroup, transaction, cancellationToken);

        Guid batchId = await SagaService.CreateBatchAsync(
            cancellationToken,
            Creator.CreateEvents(
                    new ObjectIdent(eventObject.Payload.Id, ObjectType.Group),
                    new[] { createdGroup },
                    eventObject)
                .ToArray());

        await CreateTagsAddedEvent(
            transaction,
            batchId,
            eventObject.Payload.Tags.ToList(),
            async (repo, tag) =>
                await repo.AddTagToProfileAsync(
                    tag,
                    eventObject.Payload.Id,
                    transaction,
                    cancellationToken),
            eventObject,
            firstLevelProjectionGroup.ToObjectIdent(),
            cancellationToken);

        await CreateProfileAssignmentsAsync(
            transaction,
            firstLevelProjectionGroup.ToObjectIdent(),
            eventObject.Payload.Members,
            batchId,
            eventObject,
            eventObject.Payload.Id,
            eventObject.Payload.Tags.Where(tag => tag.IsInheritable).ToList(),
            cancellationToken);

        await SagaService.ExecuteBatchAsync(batchId, cancellationToken);

        Logger.ExitMethod();
    }
}
