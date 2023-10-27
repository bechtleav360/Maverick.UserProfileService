using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

/// <summary>
///     The base class for eventObject handlers.
/// </summary>
/// <typeparam name="TEventType">
///     The type of eventObject that is handled.
/// </typeparam>
internal abstract class SyncBaseEventHandler<TEventType> : ISyncProjectionEventHandler<TEventType>
    where TEventType : class, IUserProfileServiceEvent
{
    protected readonly IProfileService ProfileService;
    protected readonly ILogger Logger;

    /// <summary>
    ///     Creates a new instance of <see cref="SyncBaseEventHandler{TEventType}" />
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> to be used.</param>
    /// <param name="profileService"></param>
    protected SyncBaseEventHandler(ILogger logger, IProfileService profileService)
    {
        Logger = logger;
        ProfileService = profileService;
    }

    /// <summary>
    ///     Handles the eventObject with eventObject specific processes.
    /// </summary>
    /// <param name="eventObject">The eventObject object that occurred.</param>
    /// <param name="eventHeader"></param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that represent the asynchronous  operation.</returns>
    protected abstract Task HandleInternalAsync(
        TEventType eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public async Task HandleEventAsync(
        TEventType eventObject,
        StreamedEventHeader streamEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            Logger.LogWarnMessage(
                "Provided domain eventObject is null. This handler {handlerType}<{eventType}> cannot proceed.",
                LogHelpers.Arguments(GetType().Name, typeof(TEventType).Name));

            throw new ArgumentNullException(nameof(eventObject), "Domain eventObject must not be null.");
        }

        if (streamEvent == null)
        {
            throw new ArgumentNullException(nameof(streamEvent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Received eventObject:\n {{eventString}}, \n with streamEvent: {streamEvent} for checkpoint\n.",
                LogHelpers.Arguments(eventObject.ToLogString(), streamEvent.ToLogString()));
        }

        Logger.LogInfoMessage(
            "Received eventObject of type {eventName}.",
            LogHelpers.Arguments(typeof(TEventType).Name));

        DateTimeOffset start = DateTimeOffset.UtcNow;

        // Each eventObject handler handles its corresponding eventObject.
        // Projection it is saved after completion of the handler. 
        try
        {
            await HandleInternalAsync(eventObject, streamEvent, cancellationToken);

            if (!await ProfileService.TrySaveProjectionStateAsync(
                    streamEvent.ToProjectionState(processingStarted: start),
                    default,
                    Logger,
                    cancellationToken))
            {
                Logger.LogInfoMessage("Could not save state.", LogHelpers.Arguments());
            }

            Logger.ExitMethod();
        }
        catch (Exception e)
        {
            try
            {
                if (!await ProfileService.TrySaveProjectionStateAsync(
                        streamEvent.ToProjectionState(e, processingStarted: start),
                        null,
                        Logger,
                        cancellationToken))
                {
                    Logger.LogInfoMessage("Could not save state (already in error)", LogHelpers.Arguments());
                }
            }
            catch (Exception exception)
            {
                // the abort can be fail because the transaction can be deleted before commitment.
                if (Logger.IsEnabledForTrace())
                {
                    Logger.LogTraceMessage(
                        "Projection state could not be saved. Error message: {errorMessage};",
                        LogHelpers.Arguments(exception.Message));
                }
            }

            throw;
        }

        Logger.LogInfoMessage(
            "Event of type {eventName} handled successfully.",
            LogHelpers.Arguments(typeof(TEventType).Name));

        Logger.ExitMethod();
    }
}
