using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
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

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="RoleTagsAddedEvent" />.
/// </summary>
internal class RoleTagsAddedFirstLevelEventHandler : FirstLevelEventHandlerBase<RoleTagsAddedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly IMapper _Mapper;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="RoleTagsAddedFirstLevelEventHandler" />.
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
    public RoleTagsAddedFirstLevelEventHandler(
        ILogger<RoleTagsAddedFirstLevelEventHandler> logger,
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
        RoleTagsAddedEvent eventObject,
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

        List<FirstLevelProjectionTag> tagsToAdd =
            eventObject.Payload.Tags.Select(x => _Mapper.Map<FirstLevelProjectionTag>(x)).ToList();

        Guid batchSagaId = await _SagaService.CreateBatchAsync(
            cancellationToken,
            _Creator.CreateEvents(
                    role.ToObjectIdent(),
                    tagsToAdd.Select(x => _Mapper.Map<TagsAdded>(x)).ToList(),
                    eventObject)
                .ToArray());

        foreach (FirstLevelProjectionTag tagToAdd in tagsToAdd)
        {
            FirstLevelProjectionTag tag;

            try
            {
                tag = await Repository.GetTagAsync(
                    tagToAdd.Id,
                    transaction,
                    cancellationToken);
            }
            catch (Exception)
            {
                await _SagaService.AbortBatchAsync(batchSagaId, cancellationToken);

                throw;
            }

            await Repository.AddTagToRoleAsync(
                _Mapper.Map<FirstLevelProjectionTagAssignment>(tag),
                role.Id,
                transaction,
                cancellationToken);
        }

        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
