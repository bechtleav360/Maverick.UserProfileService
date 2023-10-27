using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.DependencyInjection;

/// <summary>
///     Defines a resolver to determine the correct type of an event publisher.
/// </summary>
public interface IEventPublisherTypeResolver
{
    /// <summary>
    ///     Gets the publisher that will take care of the specified <paramref name="eventData" />.
    /// </summary>
    /// <param name="eventData">The event data to be published.</param>
    /// <returns>The publisher instance or <c>null</c> if none found.</returns>
    IEventPublisher GetPublisher(IUserProfileServiceEvent eventData);
}
