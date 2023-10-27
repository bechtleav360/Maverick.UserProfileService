using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Implementations;

/// <summary>
///     Implements a <see cref="IHealthStore" /> storing all states in memory.
/// </summary>
public class InMemoryHealthStore : IHealthStore
{
    private readonly ConcurrentDictionary<string, HealthState> _states =
        new ConcurrentDictionary<string, HealthState>();

    /// <inheritdoc cref="IHealthStore.SetHealthStatus" />
    public void SetHealthStatus(string key, HealthState state)
    {
        _states.AddOrUpdate(
            key,
            k => state,
            (k, v) => state);
    }

    /// <inheritdoc cref="IHealthStore.GetHealthState" />
    public HealthState GetHealthState(string key, HealthStatus defaultValue = HealthStatus.Unhealthy)
    {
        return _states.TryGetValue(key, out HealthState value)
            ? value
            : new HealthState(defaultValue, DateTime.MinValue);
    }
}
