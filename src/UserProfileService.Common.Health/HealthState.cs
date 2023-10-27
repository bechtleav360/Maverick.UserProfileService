using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health;

/// <summary>
///     Represents a single health status for use in <see cref="IHealthStore" />.
/// </summary>
public readonly struct HealthState
{
    /// <summary>
    ///     Initializes a new instance of <see cref="HealthStatus" />.
    /// </summary>
    /// <param name="status">The current health status.</param>
    /// <param name="updatedAt">Specifies when the status was created.</param>
    /// <param name="description">A description for the current status which an be and defaults to null.</param>
    /// <param name="exception">A <see cref="Exception" /> which has lead to the health status, can be null.</param>
    public HealthState(
        HealthStatus status,
        DateTime updatedAt,
        string description = null,
        Exception exception = null)
    {
        UpdatedAt = updatedAt;
        Exception = exception;
        Description = description;
        Status = status;
    }

    /// <summary>
    ///     Specifies the <see cref="HealthStatus" />.
    /// </summary>
    public HealthStatus Status { get; }

    /// <summary>
    ///     Specifies the description of the health check.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Specifies the last time the value was changed.
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    ///     Contains the thrown exception.
    /// </summary>
    public Exception Exception { get; }
}
