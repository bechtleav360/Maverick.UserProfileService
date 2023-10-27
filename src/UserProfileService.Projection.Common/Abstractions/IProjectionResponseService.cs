using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Projection.Common.Abstractions;

/// <summary>
///     Includes methods of a service to answer the underlying saga
/// </summary>
public interface IProjectionResponseService
{
    /// <summary>
    ///     Sends the response for the given event to ensure that the underlying saga/process is completed.
    /// </summary>
    /// <param name="domainEvent">The domain a response will be sent for.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ResponseAsync(
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends the exception response for the given event to ensure that the underlying saga/process is completed.
    /// </summary>
    /// <param name="domainEvent">The domain a response will be sent for.</param>
    /// <param name="exception">Exception while handling event.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ResponseAsync(
        IUserProfileServiceEvent domainEvent,
        Exception exception,
        CancellationToken cancellationToken = default);
}
