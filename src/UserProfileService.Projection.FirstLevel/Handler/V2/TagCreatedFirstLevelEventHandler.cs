using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
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
using TagCreatedEventResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.TagCreated;
using ResolvedInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="TagCreatedEvent" />.
/// </summary>
internal class TagCreatedFirstLevelEventHandler : FirstLevelEventHandlerBase<TagCreatedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly IMapper _Mapper;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="TagCreatedFirstLevelEventHandler" />.
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
    public TagCreatedFirstLevelEventHandler(
        ILogger<TagCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(
        logger,
        repository)
    {
        _SagaService = sagaService;
        _Creator = creator;
        _Mapper = mapper;
    }

    protected override async Task HandleInternalAsync(
        TagCreatedEvent eventObject,
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

        var tagProjectionObject = _Mapper.Map<FirstLevelProjectionTag>(eventObject.Payload);
        var tagCreatedEventResolved = _Mapper.Map<TagCreatedEventResolved>(eventObject.Payload);

        EventTuple tupleCreatedEventResolved = _Creator.CreateEvent(
            new ObjectIdent(tagProjectionObject.Id, ObjectType.Tag),
            tagCreatedEventResolved,
            eventObject);

        Guid batchId = await _SagaService.CreateBatchAsync(cancellationToken, tupleCreatedEventResolved);

        await Repository.CreateTag(tagProjectionObject, transaction, cancellationToken);

        await _SagaService.ExecuteBatchAsync(batchId, cancellationToken);

        Logger.ExitMethod();
    }
}
