using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

/// <summary>
///     This handler is used to process <see cref="NoActionEvent" />.
/// </summary>
internal class NoActionEventHandler : SyncBaseEventHandler<NoActionEvent>
{
    /// <summary>
    ///     Creates a new instance of <see cref="NoActionEventHandler" />
    /// </summary>
    /// <param name="logger">
    ///     <see cref="ILogger{NoActionEventHandler}" />
    /// </param>
    /// <param name="stateUserService">Repository to save projection state.</param>
    public NoActionEventHandler(
        ILogger<GroupCreatedEventHandler> logger,
        IProfileService stateUserService) : base(logger, stateUserService)
    {
    }

    protected override Task HandleInternalAsync(
        NoActionEvent eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            Logger.LogWarnMessage(
                "The variable with the name eventObject is null.",
                LogHelpers.Arguments());

            throw new ArgumentNullException($"The variable with the name {nameof(eventObject)} is null.");
        }

        // This handle is executed as a "dummy" handler to use the functionalities of the base event handler.
        // This handler is executed whenever the sync needs to skip an event,
        // but store the projections id in the database.
        // This prevents the events to be skipped from being processed multiple times.

        Logger.LogDebugMessage(
            "The event is skipped and no action is performed. Type: {type} and EventId: {eventId}.",
            LogHelpers.Arguments(eventObject.Type, eventObject.EventId));

        return Logger.ExitMethod(Task.CompletedTask);
    }
}
