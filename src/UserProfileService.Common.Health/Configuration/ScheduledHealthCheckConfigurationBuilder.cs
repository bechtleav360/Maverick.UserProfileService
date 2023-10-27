using System;
using System.Globalization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Configuration;

/// <summary>
///     Offers a fluent creation of <see cref="ScheduledHealthCheckConfiguration" />.
/// </summary>
public class ScheduledHealthCheckConfigurationBuilder
{
    private readonly ScheduledHealthCheckConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of <see cref="PushHealthCheckConfigurationBuilder" />.
    /// </summary>
    public ScheduledHealthCheckConfigurationBuilder()
    {
        _configuration = new ScheduledHealthCheckConfiguration();
    }

    /// <summary>
    ///     Sets <see cref="ScheduledHealthCheckConfiguration.FilterPredicate" />.
    /// </summary>
    /// <param name="predicate">A predicate which filters health-checks which will be pushed to the service.</param>
    /// <returns>The same instance on which the method was called.</returns>
    public ScheduledHealthCheckConfigurationBuilder SetFilterPredicate(
        Func<HealthCheckRegistration, bool> predicate)
    {
        _configuration.FilterPredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

        return this;
    }

    /// <summary>
    ///     Sets <see cref="ScheduledHealthCheckConfiguration.Delay" />.
    /// </summary>
    /// <param name="delay">The delay to wait in between publishing health states.</param>
    /// <returns>The same instance on which the method was called.</returns>
    public ScheduledHealthCheckConfigurationBuilder SetDelay(TimeSpan delay)
    {
        _configuration.Delay = delay;

        return this;
    }

    /// <summary>
    ///     Sets <see cref="ScheduledHealthCheckConfiguration.Delay" />.
    /// </summary>
    /// <remarks>
    ///     If <paramref name="timespan" /> is null, empty, cannot be parsed as number or this number is negative or zero,
    ///     the delay won't be set.
    /// </remarks>
    /// <param name="timespan">
    ///     The delay to wait in between publishing health states as a formatted string (i.e. "00:23:45"
    ///     will result in 23 minutes and 45 seconds).
    /// </param>
    /// <returns>The same instance on which the method was called.</returns>
    public ScheduledHealthCheckConfigurationBuilder SetDelay(string timespan)
    {
        if (string.IsNullOrWhiteSpace(timespan)
            || !TimeSpan.TryParse(timespan, CultureInfo.InvariantCulture, out TimeSpan converted)
            || converted <= new TimeSpan(0))
        {
            return this;
        }

        _configuration.Delay = converted;

        return this;
    }

    /// <summary>
    ///     Creates a <see cref="ScheduledHealthCheckConfiguration" /> based on the configured properties.
    /// </summary>
    /// <returns>A <see cref="ScheduledHealthCheckConfiguration" />.</returns>
    public ScheduledHealthCheckConfiguration Build()
    {
        return new ScheduledHealthCheckConfiguration(_configuration);
    }
}
