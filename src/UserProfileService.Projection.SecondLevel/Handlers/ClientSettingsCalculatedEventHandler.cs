using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="ClientSettingsCalculated" /> event.
/// </summary>
internal class ClientSettingsCalculatedEventHandler : SecondLevelEventHandlerBase<ClientSettingsCalculated>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="ClientSettingsCalculatedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper is used to map objects from one type, to another.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public ClientSettingsCalculatedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger<ClientSettingsCalculatedEventHandler> logger) : base(
        repository,
        mapper,
        streamNameResolver,
        messageInformer,
        logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        ClientSettingsCalculated domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(domainEvent.Key))
        {
            throw new ArgumentException(
                "The key of the client settings event is null or empty.",
                nameof(domainEvent));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.ProfileId))
        {
            throw new ArgumentException(
                "The profile id of the client settings event is null or empty",
                nameof(domainEvent));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.CalculatedSettings))
        {
            throw new ArgumentException(
                "The value of the client settings event is null or empty.",
                nameof(domainEvent));
        }

        // The domain profileId and the streamId has to be the same
        // otherwise another entity has to be changed!
        if (domainEvent.ProfileId != StreamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId).Id)
        {
            throw new InvalidOperationException(
                $"The {nameof(domainEvent.ProfileId)} has to be the same like the entity-stream-id that is changed. {nameof(domainEvent.ProfileId)}: {domainEvent.ProfileId}, {nameof(eventHeader.StreamId)}: {eventHeader.StreamId}");
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}, relatedEntityIdent: {relatedEntityIdent}, eventHeader: {eventHeader}",
                LogHelpers.Arguments(
                    domainEvent.ToLogString(),
                    relatedEntityIdent.ToLogString(),
                    eventHeader.ToLogString()));
        }

        Logger.LogDebugMessage(
            "The profile with the id {id} has client following client settings: {clientSettingsKey}",
            LogHelpers.Arguments(domainEvent.ProfileId.ToLogString(), domainEvent.Key.ToLogString()));

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.SetClientSettingsAsync(
                    domainEvent.ProfileId,
                    domainEvent.Key,
                    domainEvent.CalculatedSettings,
                    domainEvent.IsInherited,
                    t,
                    ct),
            eventHeader,
            cancellationToken);

        ISecondLevelProjectionProfile profile = await Repository.GetProfileAsync(
            domainEvent.ProfileId,
            cancellationToken: cancellationToken);
        
        Logger.ExitMethod();
    }
}
