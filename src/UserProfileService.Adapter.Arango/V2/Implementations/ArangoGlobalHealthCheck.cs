using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     Global health check for connection to arango database and its health status.
/// </summary>
public class ArangoGlobalHealthCheck : IHealthCheck
{
    private volatile Exception _exception;
    private volatile HealthStatus _status = HealthStatus.Healthy;

    private DateTime? _updatedAt;

    /// <summary>
    ///     Set the exception for the current health status. Status does not change.
    /// </summary>
    public Exception Exception
    {
        get => _exception;
        set => _exception = value;
    }

    /// <summary>
    ///     Sets the health status. If the status is healthy, the old error is deleted from the check.
    /// </summary>
    public HealthStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            _updatedAt = DateTime.UtcNow;
            _exception = _status == HealthStatus.Healthy ? null : _exception;
        }
    }

    private IReadOnlyDictionary<string, object> GetHealthData()
    {
        return new Dictionary<string, object>
        {
            { "UpdatedAt", _updatedAt },
            { "Status", _status }
        };
    }

    /// <inheritdoc cref="IHealthCheck" />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        IReadOnlyDictionary<string, object> healthData = GetHealthData();

        // TODO: Constant for 200sec
        if (_status == HealthStatus.Degraded && GetSecondsAfterLastUpdate() > 200)
        {
            _status = HealthStatus.Healthy;
        }

        HealthCheckResult status = _status switch
        {
            HealthStatus.Healthy =>
                HealthCheckResult.Healthy("The arango is available and can be accessed.", healthData),
            HealthStatus.Degraded =>
                HealthCheckResult.Degraded(
                    "It seems that there is a problem with the connection or request to arango. It waits 200 seconds until the status of arango is set to unhealthy.",
                    _exception,
                    healthData),
            _ =>
                HealthCheckResult.Unhealthy(
                    "The arango is not available after a long time and several attempts.",
                    _exception,
                    healthData)
        };

        return Task.FromResult(status);
    }

    /// <summary>
    ///     Returns the time in seconds since the status has not changed.
    /// </summary>
    /// <returns>Time in seconds.</returns>
    public double GetSecondsAfterLastUpdate()
    {
        DateTime updated = _updatedAt ?? DateTime.UtcNow;

        return (DateTime.UtcNow - updated).TotalSeconds;
    }
}
