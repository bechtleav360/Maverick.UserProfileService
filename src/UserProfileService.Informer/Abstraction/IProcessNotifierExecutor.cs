using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Informer.Abstraction;

/// <summary>
///     The process notifier executor executes the notification for a special event.
///     The notification can be a signalR-Signal, an message via queues or other messages
///     channels. When deriving  this interface, be aware that the constructor should only
///     contain the <see cref="IServiceProvider" />. Out of the provider all other dependencies should
///     be created. That is because the object derived out of this interface will be created dynamically
///     with Reflection.
/// </summary>
public interface IProcessNotifierExecutor
{
    /// <summary>
    ///     Executes for an special event a notification when a handler is registered.
    ///     If no handler is registered than no notification will be fired.
    /// </summary>
    /// <param name="serviceEvent">The event type for whose a notification should be created.</param>
    /// <param name="contextMessage">The context message that contains additional information for creating a notification.</param>
    Task ExecuteNotificationMessageAsync(IUserProfileServiceEvent serviceEvent, INotifyContext contextMessage);
}
