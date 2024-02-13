using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.Common.Services;

/// <summary>
///     Health check for <see cref="ProjectionBase" />.
/// </summary>
public class ProjectionServiceHealthCheck : IHealthCheck
{
    private volatile Exception _Exception;

    private volatile string _Message;
    private volatile HealthStatus _Status = HealthStatus.Healthy;

    private DateTime? _UpdatedAt;

    /// <summary>
    ///     Set the exception for the current health status. Status does not change.
    /// </summary>
    public Exception Exception
    {
        get => _Exception;
        set => _Exception = value;
    }

    /// <summary>
    ///     Set a optional message for the current health status. Status does not change.
    /// </summary>
    public string Message
    {
        get => _Message;
        set => _Message = value;
    }

    /// <summary>
    ///     Sets the health status.
    ///     If the status is healthy,
    ///     the old error and optional message are deleted from the check.
    /// </summary>
    public HealthStatus Status
    {
        get => _Status;
        set
        {
            _Status = value;
            _UpdatedAt = DateTime.UtcNow;
            _Exception = _Status == HealthStatus.Healthy ? null : _Exception;
            _Message = null;
        }
    }

    private IReadOnlyDictionary<string, object> GetHealthData()
    {
        return new Dictionary<string, object>
        {
            { "UpdatedAt", _UpdatedAt },
            { "Status", _Status }
        };
    }

    /// <inheritdoc cref="IHealthCheck" />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        IReadOnlyDictionary<string, object> healthData = GetHealthData();

        HealthCheckResult status = _Status switch
        {
            HealthStatus.Healthy =>
                HealthCheckResult.Healthy(_Message ?? "The projection is is available and running.", healthData),
            HealthStatus.Degraded =>
                HealthCheckResult.Degraded(
                    _Message
                    ?? "It seems that there is a problem with the projection. It waits 200 seconds until the status of the service is set to unhealthy.",
                    _Exception,
                    healthData),
            _ =>
                HealthCheckResult.Unhealthy(
                    _Message ?? "The projection service is not available after a long time and several attempts.",
                    _Exception,
                    healthData)
        };

        return Task.FromResult(status);
    }
}
