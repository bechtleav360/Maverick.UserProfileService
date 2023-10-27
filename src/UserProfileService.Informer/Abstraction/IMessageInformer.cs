using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Informer.Abstraction;

/// <summary>
///     The message informer get an event that implements the interface <see cref="IUserProfileServiceEvent" /> and
///     notifies a consumer when an handler is registered for this type of event.
/// </summary>
public interface IMessageInformer
{
    /// <summary>
    ///     Notifies some consumer that an event occurred. The context has
    ///     some additional information for the event, when needed. Note that not all
    ///     events will have notification. It depends if the event has an listening consumer.
    /// </summary>
    /// <param name="serviceEvent">The service event that triggered the notification for a consumer. </param>
    /// <param name="context">The context that contains additional information when needed. </param>
    Task NotifyEventOccurredAsync(IUserProfileServiceEvent serviceEvent, INotifyContext context);
}
