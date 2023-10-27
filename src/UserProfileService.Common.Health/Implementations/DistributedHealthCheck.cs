using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Implementations;

/// <summary>
///     Implements a health-check which reads from the <see cref="IDistributedHealthStatusStore" />.
/// </summary>
public class DistributedHealthCheck : IHealthCheck
{
    private readonly IDistributedHealthStatusStore _healthStore;
    private readonly string _workerName;

    /// <summary>
    ///     Initializes a new instance of <see cref="DistributedHealthCheck" />.
    /// </summary>
    /// <param name="healthStore">The <see cref="IDistributedHealthStatusStore" /> to read from.</param>
    /// <param name="workerName">The name of the worker to check health for.</param>
    public DistributedHealthCheck(IDistributedHealthStatusStore healthStore, string workerName)
    {
        _healthStore = healthStore;
        _workerName = workerName;
    }

    /// <inheritdoc cref="IHealthCheck.CheckHealthAsync" />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return _healthStore.GetHealthStatusAsync(
            _workerName,
            context.Registration.FailureStatus,
            cancellationToken);
    }
}
