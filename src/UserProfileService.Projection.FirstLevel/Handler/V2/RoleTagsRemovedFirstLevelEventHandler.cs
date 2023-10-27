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
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="RoleTagsRemovedEvent" />.
/// </summary>
internal class RoleTagsRemovedFirstLevelEventHandler : FirstLevelEventHandlerBase<RoleTagsRemovedEvent>
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly IMapper _Mapper;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="RoleTagsRemovedFirstLevelEventHandler" />.
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
    public RoleTagsRemovedFirstLevelEventHandler(
        ILogger<RoleTagsRemovedFirstLevelEventHandler> logger,
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
        RoleTagsRemovedEvent eventObject,
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
            eventObject.Payload.ResourceId,
            null,
            cancellationToken);

        List<string> tagsToRemoveIds = eventObject.Payload.Tags.ToList();

        Guid batchSagaId = await _SagaService.CreateBatchAsync(
            cancellationToken,
            _Creator.CreateEvent(
                role.ToObjectIdent(),
                _Mapper.Map<TagsRemoved>(eventObject),
                eventObject));

        foreach (string tagToRemoveId in tagsToRemoveIds)
        {
            FirstLevelProjectionTag tag;

            try
            {
                tag = await Repository.GetTagAsync(
                    tagToRemoveId,
                    transaction,
                    cancellationToken);
            }
            catch (Exception)
            {
                await _SagaService.AbortBatchAsync(batchSagaId, cancellationToken);

                throw;
            }

            try
            {
                await Repository.RemoveTagFromRoleAsync(tag.Id, role.Id, transaction, cancellationToken);
            }
            catch (InstanceNotFoundException e)
            {
                Logger.LogErrorMessage(e, "Unable to remove tag from role.", LogHelpers.Arguments());
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(e, "Unable to remove tag from role.", LogHelpers.Arguments());
                await _SagaService.AbortBatchAsync(batchSagaId, cancellationToken);

                throw;
            }
        }

        await _SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);
        Logger.ExitMethod();
    }
}
