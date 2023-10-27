using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

/// <summary>
///     Includes methods of a second-level projection event handler that will take of a specific event type.
/// </summary>
/// <typeparam name="TEvent">The type of the event to be processed.</typeparam>
public interface ISecondLevelAssignmentEventHandler<in TEvent>: ISecondLevelAssignmentEventHandler
    where TEvent : IUserProfileServiceEvent
{
    /// <summary>
    ///     Take care of a provided <paramref name="domainEvent" />, if necessary.
    /// </summary>
    /// <param name="domainEvent">The domain to be handled.</param>
    /// <param name="eventHeader">Containing further information about the <paramref name="domainEvent" />.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="domainEvent" /> is <c>null</c>br />-or-<br />
    ///     <paramref name="eventHeader" /> is <c>null</c>
    /// </exception>
    Task HandleEventAsync(
        TEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default);
}
