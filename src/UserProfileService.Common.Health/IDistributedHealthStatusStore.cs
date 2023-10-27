using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Common.Health.Report;

namespace UserProfileService.Common.Health;

/// <summary>
///     Represents a HealthStore for distributed external systems.
/// </summary>
public interface IDistributedHealthStatusStore
{
    /// <summary>
    ///     Adds a new received HealthStatus.
    /// </summary>
    /// <param name="healthStatus"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task AddHealthStatusAsync(HealthCheckMessage healthStatus, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a the result of the HealthCheck for the specified worker name.
    /// </summary>
    /// <param name="workerName">The name of the worker.</param>
    /// <param name="failureStatus">The lowest status the health check can result in.</param>
    /// <param name="cancellationToken">A token which can be used in order to stop the operation. Defaults to </param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task<HealthCheckResult> GetHealthStatusAsync(
        string workerName,
        HealthStatus failureStatus = HealthStatus.Degraded,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Cleans up all unnecessary
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    Task CleanupAsync();
}
