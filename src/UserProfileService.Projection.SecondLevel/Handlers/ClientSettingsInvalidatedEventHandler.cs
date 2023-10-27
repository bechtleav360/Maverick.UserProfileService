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
///     Processes an <see cref="ClientSettingsInvalidated" /> event.
/// </summary>
internal class ClientSettingsInvalidatedEventHandler : SecondLevelEventHandlerBase<ClientSettingsInvalidated>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="ClientSettingsInvalidatedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper is used to map objects from one type, to another.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="logger">The logger to be used.</param>
    public ClientSettingsInvalidatedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger<ClientSettingsInvalidatedEventHandler> logger) : base(
        repository,
        mapper,
        streamNameResolver,
        messageInformer,
        logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        ClientSettingsInvalidated domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        // if Keys is empty, then there are no ClientSettings for the profile
        // and all remaining ClientSettings should be deleted, so this is a valid input
        if (domainEvent.Keys == null)
        {
            throw new ArgumentException(
                "The keys of the client settings event is null.",
                nameof(domainEvent));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.ProfileId))
        {
            throw new ArgumentException(
                "The profile id of the client settings event is null or empty",
                nameof(domainEvent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}, relatedEntityIdent: {relatedEntityIdent}, eventHeader: {eventHeader }",
                LogHelpers.Arguments(
                    domainEvent.ToLogString(),
                    relatedEntityIdent.ToLogString(),
                    eventHeader.ToLogString()));
        }

        Logger.LogDebugMessage(
            "The client settings keys {clientSettingsKey} are now valid for the profile id: {profileId}",
            LogHelpers.Arguments(domainEvent.Keys.ToLogString(), domainEvent.ProfileId.ToLogString()));

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.InvalidateClientSettingsFromProfile(
                    domainEvent.ProfileId,
                    domainEvent.Keys,
                    t,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
