using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Projection.VolatileData.DependencyInjection;

/// <summary>
///     Represents a config builder to register a volatile data store to a DI container.
/// </summary>
public interface IVolatileDataStoreConfigBuilder
{
    /// <summary>
    ///     Sets a <typeparamref name="TEvent" /> as supported event type of the related event publisher.
    /// </summary>
    /// <typeparam name="TEvent">
    ///     The type of the <see cref="IUserProfileServiceEvent" /> to be processed by the related event
    ///     publisher.
    /// </typeparam>
    /// <returns>The modified configuration builder.</returns>
    IVolatileDataStoreConfigBuilder SupportEvent<TEvent>()
        where TEvent : IUserProfileServiceEvent;
}
