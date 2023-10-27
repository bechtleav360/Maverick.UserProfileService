using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Common.Health.Services;

namespace UserProfileService.Common.Health.Configuration;

/// <summary>
///     Stores configuration for <see cref="PushHealthService" />.
/// </summary>
public class PushHealthCheckConfiguration
{
    /// <summary>
    ///     Specifies the delay between publishing health information.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Specifies the filter for health checks to publish.
    /// </summary>
    public Func<HealthCheckRegistration, bool> FilterPredicate { get; set; } = f => true;

    /// <summary>
    ///     Specifies the name of the current instance. Should be unique.
    /// </summary>
    public string InstanceName { get; set; } = Guid.NewGuid().ToString("D");

    /// <summary>
    ///     Specifies the name of the worker, it should be the same for multiple instances of the same executable.
    /// </summary>
    public string WorkerName { get; set; }

    /// <summary>
    ///     Initializes a new instance with everything set to a default-value.
    /// </summary>
    public PushHealthCheckConfiguration()
    {
    }

    /// <summary>
    ///     Initializes a new instance where every value is copied from the given configuration.
    /// </summary>
    /// <param name="old">THe <see cref="PushHealthCheckConfiguration" /> to copy.</param>
    public PushHealthCheckConfiguration(PushHealthCheckConfiguration old)
    {
        FilterPredicate = old.FilterPredicate;
        Delay = old.Delay;
        InstanceName = old.InstanceName;
        WorkerName = old.WorkerName;
    }
}
