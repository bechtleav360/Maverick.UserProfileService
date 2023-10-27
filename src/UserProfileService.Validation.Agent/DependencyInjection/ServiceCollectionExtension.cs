#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.EventCollector.Configuration;
using UserProfileService.Messaging;
using UserProfileService.Messaging.DependencyInjection;

namespace UserProfileService.EventCollector.DependencyInjection;

/// <summary>
///     Contains methods to extend <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Adds the collector event agent.
    ///     MassTransit should already be registered.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="configuration">Section of configuration for agent.</param>
    /// <returns>
    ///     <see cref="IServiceCollection" />
    /// </returns>
    public static IServiceCollection AddEventCollectorAgent(
        this IServiceCollection services,
        IConfigurationSection configuration)
    {
        services.Configure<EventCollectorConfiguration>(configuration);

        return services;
    }

    /// <summary>
    ///     Adds the collector event agent.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="configuration">Section of configuration for agent.</param>
    /// <param name="eventCollectorStoreFactory">Factory to use to inject <see cref="IEventCollectorStore" />.</param>
    /// <param name="metadata">metadata for the current application, used to configure messaging</param>
    /// <param name="messagingConfig">root-section for all messaging related configuration</param>
    /// <param name="assemblies">
    ///     assemblies from which all consumers shall be registered. Defaults to entry-assembly if null.
    /// </param>
    /// <returns>
    ///     <see cref="IServiceCollection" />
    /// </returns>
    public static IServiceCollection AddEventCollectorAgent(
        this IServiceCollection services,
        IConfigurationSection configuration,
        Func<IServiceProvider, IEventCollectorStore> eventCollectorStoreFactory,
        ServiceMessagingMetadata metadata,
        IConfiguration messagingConfig,
        IEnumerable<Assembly>? assemblies = null)
    {
        services.AddEventCollectorAgent(configuration);

        services
            .AddMessaging(
                metadata,
                messagingConfig,
                assemblies)
            .AddTransient(eventCollectorStoreFactory);

        return services;
    }
}
