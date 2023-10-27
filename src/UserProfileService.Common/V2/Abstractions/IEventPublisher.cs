using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Publishes an <see cref="IUserProfileServiceEvent" />
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    ///     If <c>true</c>, this instance is defined as default publisher.
    /// </summary>
    bool IsDefault { get; }

    /// <summary>
    ///     The type name of the event publisher.
    /// </summary>
    string Type { get; }

    /// <summary>
    ///     Publishes the <paramref name="eventData" />.
    /// </summary>
    /// <param name="eventData">The event data to be published.</param>
    /// <param name="context">A context under which the method is been executed.</param>
    /// <param name="cancellationToken">A token to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task PublishAsync(
        IUserProfileServiceEvent eventData,
        EventPublisherContext context,
        CancellationToken cancellationToken = default);
}
