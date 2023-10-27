using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

/// <summary>
///     Includes methods of a second-level projection event handler.
/// </summary>
public interface ISecondLevelAssignmentEventHandler
{
    /// <summary>
    ///     Take care of a provided <paramref name="domainEvent" />, if necessary.
    /// </summary>
    /// <param name="domainEvent">The domain to be handled.</param>
    /// <param name="eventHeader">Containing further information about the <paramref name="domainEvent" />.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleEventAsync(
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default);
}
