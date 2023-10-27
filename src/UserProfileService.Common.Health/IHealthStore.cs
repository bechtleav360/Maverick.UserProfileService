using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health;

/// <summary>
///     An store which can be used in order to store directly toggleable health states.
/// </summary>
public interface IHealthStore
{
    /// <summary>
    ///     Sets the state of the given health-check.
    /// </summary>
    /// <param name="key">The key of the health state.</param>
    /// <param name="status">The new health state.</param>
    void SetHealthStatus(string key, HealthState status);

    /// <summary>
    ///     Returns the state stored for the given key.
    /// </summary>
    /// <param name="key">The key of the health state.</param>
    /// <param name="defaultValue">Specifies the default value for the health status.</param>
    /// <returns>A Tuple containing the health state and the time when it was last modified.</returns>
    HealthState GetHealthState(string key, HealthStatus defaultValue = HealthStatus.Unhealthy);
}
