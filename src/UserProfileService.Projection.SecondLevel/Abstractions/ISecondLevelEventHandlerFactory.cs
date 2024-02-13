using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Projection.SecondLevel.Abstractions;

/// <summary>
///     Contains methods to create instances of second-level-projection handlers, that will handle
///     <see cref="IUserProfileServiceEvent" />s.
/// </summary>
public interface ISecondLevelEventHandlerFactory
{
    /// <summary>
    ///     Creates Event handler which handles event of type <typeparamref name="TEvent" />
    /// </summary>
    /// <typeparam name="TEvent">Event type supported by the generated event handler</typeparam>
    /// <returns> Event handler <see cref="ISecondLevelEventHandler" /></returns>
    ISecondLevelEventHandler CreateHandler<TEvent>() where TEvent : IUserProfileServiceEvent;
}
