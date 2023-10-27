using System;
using System.Collections.Generic;
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
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="ProfileClientSettingsSetEvent" />.
/// </summary>
internal class ProfileClientSettingsSetFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<ProfileClientSettingsSetEvent>
{
    /// <summary>
    ///     Creates an instance of the object <see cref="ProfileClientSettingsSetFirstLevelEventHandler" />.
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
    public ProfileClientSettingsSetFirstLevelEventHandler(
        ILogger<ProfileClientSettingsSetFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(logger, repository, sagaService, mapper, creator)
    {
    }

    protected override async Task HandleInternalAsync(
        ProfileClientSettingsSetEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
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
                "ProfileClientSettingsSetEvent: {profileClientSettingsSetEvent}",
                eventObject.ToLogString().AsArgumentList());
        }

        Guid batchSagaId = await SagaService.CreateBatchAsync(cancellationToken);

        Logger.LogInfoMessage(
            "The batch id for the client settings event: {batchSagaId}",
            batchSagaId.ToLogString().AsArgumentList());

        IFirstLevelProjectionProfile profile = await Repository.GetProfileAsync(
            eventObject.Payload.Resource.Id,
            transaction,
            cancellationToken);

        IEnumerable<EventTuple> clientSettingTuple = await SetClientSettingsAndRecalculateTupleAsync(
            profile.ToObjectIdent(),
            transaction,
            eventObject,
            eventObject.Payload.Key,
            eventObject.Payload.Settings.ToString(),
            cancellationToken);

        await SagaService.AddEventsAsync(batchSagaId, clientSettingTuple, cancellationToken);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "The new recalculated event tuple for the profileId {profileId}: {clientSettingTuple}",
                LogHelpers.Arguments(eventObject.Payload.Resource.ToLogString(), clientSettingTuple.ToLogString()));
        }

        await RecalculatedClientSettingsForChildren(
            profile.ToObjectIdent(),
            transaction,
            batchSagaId,
            eventObject,
            eventObject.Payload.Key,
            cancellationToken);

        await SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
