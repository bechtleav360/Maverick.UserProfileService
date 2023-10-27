using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Abstractions;

/// <summary>
///     Includes methods of a first level projection event handler that will take of a specific event type.
/// </summary>
/// <typeparam name="TEventType">The type of the event to be processed.</typeparam>
internal interface IFirstLevelProjectionEventHandler<TEventType>: IFirstLevelProjectionEventHandler
    where TEventType : IUserProfileServiceEvent
{
    /// <summary>
    ///     Take care of a provided <paramref name="eventObject" />, if necessary.
    /// </summary>
    /// <param name="eventObject">The event to be handled.</param>
    /// <param name="eventHeader">Containing further information about the <paramref name="eventObject" />.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleEventAsync(
        TEventType eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default);
}
