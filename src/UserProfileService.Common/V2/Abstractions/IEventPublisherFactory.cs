using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Represents a factory to create/return instances of <see cref="IEventPublisher" />s that are capable to handle
///     provided event types.
/// </summary>
public interface IEventPublisherFactory
{
    /// <summary>
    ///     Returns a suitable <see cref="IEventPublisher" /> instance that is able to publish the provided
    ///     <paramref name="eventData" />.
    /// </summary>
    /// <remarks>
    ///     This method won't publish the event itself - it will only return a publisher to do that.<br />
    ///     If no custom publisher can be found, the default one will be used.
    /// </remarks>
    /// <param name="eventData">The event data to be published</param>
    /// <returns>A suitable <see cref="IEventPublisher" /> instance that can be used to publish <paramref name="eventData" />.</returns>
    IEventPublisher GetPublisher(IUserProfileServiceEvent eventData);
}
