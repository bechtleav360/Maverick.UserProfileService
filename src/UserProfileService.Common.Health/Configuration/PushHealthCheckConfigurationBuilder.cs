using System;
using System.Globalization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Configuration;

/// <summary>
///     Offers a fluent creation of <see cref="PushHealthCheckConfiguration" />.
/// </summary>
public class PushHealthCheckConfigurationBuilder
{
    private readonly PushHealthCheckConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of <see cref="PushHealthCheckConfigurationBuilder" />.
    /// </summary>
    public PushHealthCheckConfigurationBuilder()
    {
        _configuration = new PushHealthCheckConfiguration();
    }

    /// <summary>
    ///     Sets <see cref="PushHealthCheckConfiguration.FilterPredicate" />.
    /// </summary>
    /// <param name="predicate">A predicate which filters health-checks which will be pushed to the service.</param>
    /// <returns>The same instance on which the method was called.</returns>
    public PushHealthCheckConfigurationBuilder SetFilterPredicate(Func<HealthCheckRegistration, bool> predicate)
    {
        _configuration.FilterPredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

        return this;
    }

    /// <summary>
    ///     Sets <see cref="PushHealthCheckConfiguration.Delay" />.
    /// </summary>
    /// <param name="delay">The delay to wait in between publishing health states.</param>
    /// <returns>The same instance on which the method was called.</returns>
    public PushHealthCheckConfigurationBuilder SetDelay(TimeSpan delay)
    {
        _configuration.Delay = delay;

        return this;
    }

    /// <summary>
    ///     Sets <see cref="PushHealthCheckConfiguration.Delay" />.
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
    public PushHealthCheckConfigurationBuilder SetDelay(string timespan)
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
    ///     Sets <see cref="PushHealthCheckConfiguration.InstanceName" />.
    /// </summary>
    /// <param name="instanceName">An unique name for this instance.</param>
    /// <returns>The same instance on which the method was called.</returns>
    public PushHealthCheckConfigurationBuilder SetInstanceName(string instanceName)
    {
        if (instanceName == null)
        {
            throw new ArgumentNullException(nameof(instanceName));
        }

        if (string.IsNullOrWhiteSpace(instanceName))
        {
            throw new ArgumentException(
                "The instance name must not be empty or only contain whitespaces",
                nameof(instanceName));
        }

        _configuration.InstanceName = instanceName;

        return this;
    }

    /// <summary>
    ///     Sets <see cref="PushHealthCheckConfiguration.WorkerName" />.
    /// </summary>
    /// <param name="workerName">A shared name for all workers of this type.</param>
    /// <returns>The same instance on which the method was called.</returns>
    public PushHealthCheckConfigurationBuilder SetWorkerName(string workerName)
    {
        if (workerName == null)
        {
            throw new ArgumentNullException(nameof(workerName));
        }

        if (string.IsNullOrWhiteSpace(workerName))
        {
            throw new ArgumentException(
                "The worker name must not be empty or only contain whitespaces",
                nameof(workerName));
        }

        _configuration.WorkerName = workerName;

        return this;
    }

    /// <summary>
    ///     Creates a <see cref="PushHealthCheckConfiguration" /> based on the configured properties.
    /// </summary>
    /// <returns>A <see cref="PushHealthCheckConfiguration" />.</returns>
    public PushHealthCheckConfiguration Build()
    {
        return new PushHealthCheckConfiguration(_configuration);
    }
}
