using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Implementations;

/// <summary>
///     Implements an <see cref="IHealthCheck" /> reading its data from a <see cref="IHealthStore" />.
/// </summary>
public class HealthStateCheck : IHealthCheck
{
    private readonly IHealthStore _healthStore;
    private readonly string _key;

    /// <summary>
    ///     Initializes a new <see cref="HealthStateCheck" />.
    /// </summary>
    /// <param name="healthStore">The <see cref="IHealthStore" /> to read from.</param>
    /// <param name="key">The key to represent with this check.</param>
    public HealthStateCheck(IHealthStore healthStore, string key)
    {
        _healthStore = healthStore;
        _key = key;
    }

    /// <inheritdoc cref="IHealthCheck.CheckHealthAsync" />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        HealthState state = _healthStore.GetHealthState(_key, context.Registration.FailureStatus);
        HealthStatus status = state.Status;

        if (status < context.Registration.FailureStatus)
        {
            status = context.Registration.FailureStatus;
        }

        var data = new Dictionary<string, object>
        {
            { nameof(HealthState.UpdatedAt), state.UpdatedAt },
            { nameof(HealthCheckRegistration.FailureStatus), context.Registration.FailureStatus },
            { nameof(HealthCheckResult.Status), state.Status }
        };

        return Task.FromResult(
            new HealthCheckResult(
                status,
                state.Description,
                state.Exception,
                new ReadOnlyDictionary<string, object>(data)));
    }
}
