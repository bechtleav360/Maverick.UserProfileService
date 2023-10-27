using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Common.Health.Implementations;

namespace UserProfileService.Common.Health.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IServiceCollection" />s.
/// </summary>
public static class HealthChecksBuilderExtensions
{
    /// <summary>
    ///     Sets up the automatic health publisher.
    /// </summary>
    /// <param name="builder">The <see cref="HealthChecksBuilder" /> to use.</param>
    /// <param name="name">The name of the health service.</param>
    /// <param name="key">The key of the stored health check.</param>
    /// <param name="failureStatus">The worst status which will be published.</param>
    /// <param name="tags">The tags to apply for the health checks.</param>
    /// <returns><paramref name="builder" />.</returns>
    public static IHealthChecksBuilder AddStoredHealthCheck(
        this IHealthChecksBuilder builder,
        string name,
        string key,
        HealthStatus? failureStatus = null,
        IEnumerable<string> tags = null)
    {
        builder.AddTypeActivatedCheck<HealthStateCheck>(name, failureStatus, tags ?? Array.Empty<string>(), key);

        return builder;
    }

    /// <summary>
    ///     Sets up a <see cref="IHealthCheck" /> querying the <see cref="IDistributedHealthStatusStore" />.
    /// </summary>
    /// <param name="builder">The <see cref="HealthChecksBuilder" /> to use.</param>
    /// <param name="name">The name of the health service.</param>
    /// <param name="key">The key of the health check.</param>
    /// <param name="failureStatus">The worst status which will be published.</param>
    /// <param name="tags">The tags to apply for the health checks.</param>
    /// <returns><paramref name="builder" />.</returns>
    public static IHealthChecksBuilder AddDistributedHealthCheck(
        this IHealthChecksBuilder builder,
        string name,
        string key,
        HealthStatus? failureStatus = null,
        IEnumerable<string> tags = null)
    {
        builder.AddTypeActivatedCheck<DistributedHealthCheck>(
            name,
            failureStatus,
            tags ?? Array.Empty<string>(),
            key);

        return builder;
    }
}
