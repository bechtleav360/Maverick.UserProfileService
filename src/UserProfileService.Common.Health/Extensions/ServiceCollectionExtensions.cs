using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Common.Health.Configuration;
using UserProfileService.Common.Health.Implementations;
using UserProfileService.Common.Health.Services;

namespace UserProfileService.Common.Health.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IServiceCollection" />s.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Sets up the automatic health publisher.
    /// </summary>
    /// <param name="services">The service collection to set up the health publisher in.</param>
    /// <param name="configuration">An action in which the health publisher can be configured.</param>
    /// <returns><paramref name="services" />.</returns>
    public static IServiceCollection AddHealthPublisher(
        this IServiceCollection services,
        Action<PushHealthCheckConfigurationBuilder> configuration)
    {
        var configurationBuilder = new PushHealthCheckConfigurationBuilder();
        configuration?.Invoke(configurationBuilder);

        services.AddSingleton(configurationBuilder.Build());
        services.AddHostedService<PushHealthService>();

        return services;
    }

    /// <summary>
    ///     Sets up the automatic health publisher.
    /// </summary>
    /// <param name="services">The service collection to set up the health publisher in.</param>
    /// <param name="configuration">An action in which the health publisher can be configured.</param>
    /// <returns><paramref name="services" />.</returns>
    public static IServiceCollection AddScheduledHealthChecks(
        this IServiceCollection services,
        Action<ScheduledHealthCheckConfigurationBuilder> configuration)
    {
        var configurationBuilder = new ScheduledHealthCheckConfigurationBuilder();
        configuration?.Invoke(configurationBuilder);

        services.AddSingleton(configurationBuilder.Build());
        services.AddHealthStatusStore();
        services.AddHostedService<ScheduledHealthCheckService>();

        return services;
    }

    /// <summary>
    ///     Sets up a <see cref="IHealthStore" />.
    /// </summary>
    /// <param name="services">The service collection to set up the health store in.</param>
    /// <returns><paramref name="services" />.</returns>
    public static IServiceCollection AddHealthStatusStore(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IHealthStore, InMemoryHealthStore>();

        return services;
    }

    /// <summary>
    ///     Register a Type as Health-Check
    /// </summary>
    /// <param name="builder">Health-Check-Builder to configure</param>
    /// <param name="name">public name of this Check</param>
    /// <param name="tags">Tags for this Health-Check</param>
    /// <param name="timeout">Timeout after which the Check is considered Failed</param>
    /// <typeparam name="THealthCheck">Type implementing <see cref="IHealthCheck" /></typeparam>
    /// <returns>configured instance of <paramref name="builder" /></returns>
    /// <exception cref="ArgumentNullException">
    ///     thrown when any non-nullable argument is null
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     thrown when any argument is out of range of the allowed values
    /// </exception>
    public static IHealthChecksBuilder AddMaverickHealthCheck<THealthCheck>(
        this IHealthChecksBuilder builder,
        string name,
        IEnumerable<string> tags,
        TimeSpan? timeout = null)
        where THealthCheck : class, IHealthCheck
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder), "builder cannot be null");
        }

        switch (name)
        {
            case null:
                throw new ArgumentNullException(nameof(name), "health-check name cannot be null");
            case "":
                throw new ArgumentOutOfRangeException(nameof(name), "health-check name cannot be empty");
        }

        if (tags is null)
        {
            throw new ArgumentNullException(nameof(tags), "health-check tags cannot be null");
        }

        List<string> actualTags = tags.ToList();

        if (actualTags.Any() == false)
        {
            throw new ArgumentOutOfRangeException(nameof(tags), "health-check tags cannot be empty");
        }

        // register in DI so it can be created
        builder.Services.TryAddSingleton<THealthCheck>();

        // register as HealthCheck for middleware
        builder.AddCheck<THealthCheck>(
            name,
            HealthStatus.Unhealthy,
            actualTags,
            timeout ?? TimeSpan.FromSeconds(3));

        return builder;
    }

    /// <summary>
    ///     Add Health-Checks to the given <see cref="IServiceCollection" />
    /// </summary>
    /// <param name="services">service-collection to configure</param>
    /// <param name="setupHealthChecks">action to add and configure the Health-Checks</param>
    /// <exception cref="ArgumentNullException">thrown when any argument is null</exception>
    /// <returns>modified instance of <paramref name="services" /></returns>
    public static IServiceCollection AddMaverickHealthChecks(
        this IServiceCollection services,
        Action<IHealthChecksBuilder> setupHealthChecks)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (setupHealthChecks is null)
        {
            throw new ArgumentNullException(nameof(setupHealthChecks));
        }

        IHealthChecksBuilder builder = services.AddHealthChecks();

        setupHealthChecks(builder);

        return services;
    }

    /// <summary>
    ///     Register the given Type as Liveness-Check
    /// </summary>
    /// <param name="builder">Health-Check-Builder to configure</param>
    /// <param name="name">public name of this Check</param>
    /// <param name="timeout">Timeout after which the Check is considered Failed</param>
    /// <typeparam name="THealthCheck">Type implementing <see cref="IHealthCheck" /></typeparam>
    /// <returns>configured instance of <paramref name="builder" /></returns>
    public static IHealthChecksBuilder AddMaverickLivenessCheck<THealthCheck>(
        this IHealthChecksBuilder builder,
        string name,
        TimeSpan? timeout = null)
        where THealthCheck : class, IHealthCheck
    {
        return AddMaverickHealthCheck<THealthCheck>(builder, name, new[] { HealthCheckTags.Liveness }, timeout);
    }

    /// <summary>
    ///     Register the given Type as Readiness-Check
    /// </summary>
    /// <param name="builder">Health-Check-Builder to configure</param>
    /// <param name="name">public name of this Check</param>
    /// <param name="timeout">Timeout after which the Check is considered Failed</param>
    /// <typeparam name="THealthCheck">Type implementing <see cref="IHealthCheck" /></typeparam>
    /// <returns>configured instance of <paramref name="builder" /></returns>
    public static IHealthChecksBuilder AddMaverickReadinessCheck<THealthCheck>(
        this IHealthChecksBuilder builder,
        string name,
        TimeSpan? timeout = null)
        where THealthCheck : class, IHealthCheck
    {
        return AddMaverickHealthCheck<THealthCheck>(builder, name, new[] { HealthCheckTags.Readiness }, timeout);
    }

    /// <summary>
    ///     Register the given Type as State-Check
    /// </summary>
    /// <param name="builder">Health-Check-Builder to configure</param>
    /// <param name="name">public name of this Check</param>
    /// <param name="timeout">Timeout after which the Check is considered Failed</param>
    /// <typeparam name="THealthCheck">Type implementing <see cref="IHealthCheck" /></typeparam>
    /// <returns>configured instance of <paramref name="builder" /></returns>
    public static IHealthChecksBuilder AddMaverickStateCheck<THealthCheck>(
        this IHealthChecksBuilder builder,
        string name,
        TimeSpan? timeout = null)
        where THealthCheck : class, IHealthCheck
    {
        return AddMaverickHealthCheck<THealthCheck>(builder, name, new[] { HealthCheckTags.State }, timeout);
    }
}
