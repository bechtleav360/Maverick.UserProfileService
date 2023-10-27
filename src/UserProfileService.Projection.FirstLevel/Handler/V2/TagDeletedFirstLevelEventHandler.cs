using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using ObjectTypeResolved = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="TagDeletedEvent" />.
/// </summary>
internal class TagDeletedFirstLevelEventHandler : FirstLevelEventHandlerBase<TagDeletedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="TagDeletedFirstLevelEventHandler" />.
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
    public TagDeletedFirstLevelEventHandler(
        ILogger<TagDeletedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(
        logger,
        repository)
    {
        _SagaService = sagaService;
        _Creator = creator;
    }

    protected override async Task HandleInternalAsync(
        TagDeletedEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        string tagId = eventObject.Payload.Id;

        Guid batchSagaId = await _SagaService.CreateBatchAsync(cancellationToken);
        FirstLevelProjectionTag tag = null;

        await ExecuteSafelyAsync(
            async () =>
            {
                tag =
                    await Repository.GetTagAsync(tagId, transaction, cancellationToken);
            },
            async () => { await _SagaService.AbortBatchAsync(batchSagaId, cancellationToken); });

        IList<ObjectIdent> tagAssignmentsUsedByProfile = null;

        await ExecuteSafelyAsync(
            async () =>
            {
                tagAssignmentsUsedByProfile =
                    await Repository.GetAssignedObjectsFromTagAsync(tag.Id, transaction, cancellationToken);
            },
            async () => { await Task.CompletedTask; },
            false);

        await Repository.DeleteTagAsync(
            tag.Id,
            transaction,
            cancellationToken);

        if (tagAssignmentsUsedByProfile != null)
        {
            foreach (ObjectIdent profilesTagToDelete in tagAssignmentsUsedByProfile)
            {
                await _SagaService.AddEventsAsync(
                    batchSagaId,
                    _Creator.CreateEvents(
                        profilesTagToDelete,
                        new[]
                        {
                            new TagDeleted
                            {
                                TagId = eventObject.Payload.Id
                            }
                        },
                        eventObject),
                    cancellationToken);
            }
        }

        await _SagaService.AddEventsAsync(
            batchSagaId,
            _Creator.CreateEvents(
                new ObjectIdent(eventObject.Payload.Id, ObjectType.Tag),
                new[]
                {
                    new EntityDeleted
                    {
                        Id = eventObject.Payload.Id
                    }
                },
                eventObject),
            cancellationToken);

        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);
    }
}
