using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Sync.Projection.Abstractions;

/// <summary>
///     Includes methods of a sync projection event handler.
/// </summary>
internal interface ISyncProjectionEventHandler<in TEventType>
    where TEventType : IUserProfileServiceEvent
{
    /// <summary>
    ///     Take care of a provided <paramref name="eventObject" />, if necessary.
    /// </summary>
    /// <param name="eventObject">The event to be handled.</param>
    /// <param name="eventHeader">The event header contains information about the origins and content of a previously written event.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleEventAsync(
        TEventType eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default);
}
