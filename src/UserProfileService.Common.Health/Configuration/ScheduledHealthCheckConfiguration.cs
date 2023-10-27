using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Common.Health.Services;

namespace UserProfileService.Common.Health.Configuration;

/// <summary>
///     Stores a Configuration for <see cref="ScheduledHealthCheckService" />
/// </summary>
public class ScheduledHealthCheckConfiguration
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
    ///     Initializes a new instance of <see cref="ScheduledHealthCheckConfiguration" /> with default values.
    /// </summary>
    public ScheduledHealthCheckConfiguration()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ScheduledHealthCheckConfiguration" /> and copies all values from
    ///     <paramref name="old" />.
    /// </summary>
    /// <param name="old">The <see cref="ScheduledHealthCheckConfiguration" /> to copy.</param>
    public ScheduledHealthCheckConfiguration(ScheduledHealthCheckConfiguration old)
    {
        FilterPredicate = old.FilterPredicate;
        Delay = old.Delay;
    }
}
