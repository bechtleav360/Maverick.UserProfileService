using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Configurations.Abstraction;
using UserProfileService.Sync.Utilities;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Registers all sync provider configuration and dependencies that are needed to sync from/to
///     the third party systems.
/// </summary>
public static class SyncProviderConfigurationRegistration
{

    /// <summary>
    /// Registers all sync configuration provider that implements the <see cref="ISyncProviderConfigurationRegistration"/> interface.
    /// </summary>
    /// <param name="serviceCollection">The service collection where the provider are registered.</param>
    /// <param name="assemblies">The assemblies where the providers are situated.</param>
    /// <param name="mainConfiguration">The main configuration where the necessary configuration is extracted.</param>
    /// <param name="logger">The logger that is needed for logging purposes.</param>
    /// <returns>The <see cref="IServiceCollection"/> that contains the registered configuration provider.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddSyncConfigurationProvider(
        this IServiceCollection serviceCollection,
        Assembly[] assemblies,
        IConfiguration mainConfiguration,
        ILogger logger)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }
        
        logger.EnterMethod();
        
        if (serviceCollection == null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

        if (assemblies == null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        List<string> configurationToBeUser = assemblies
                                             .SelectMany(a => a.GetTypes())
                                             .Where(
                                                 p => !string.IsNullOrEmpty(
                                                     p.GetCustomAttribute<SynchConfigurationProviderAttribute>()
                                                      ?.SyncConfigName))
                                             .Select(
                                                 p => p.GetCustomAttribute<SynchConfigurationProviderAttribute>().SyncConfigName)
                                             .ToList();

        Dictionary<string, IConfigurationSection> systemConfigurationProviders = mainConfiguration
                                                                                 .GetSection(ConfigSectionsConstants.LdapConfigurationSectionName)
                                                                                 .Get<Dictionary<string, IConfigurationSection>>()
                                                                                 .Where(p => configurationToBeUser.Contains(p.Key, StringComparer.InvariantCulture))
                                                                                 .ToDictionary(
                                                                                     kv => kv.Key,
                                                                                     kv => kv.Value,
                                                                                     StringComparer.OrdinalIgnoreCase);
        
        Type[] implementedProviderRegistrations = 
            assemblies
                .SelectMany(a => a.GetTypes()
                                  .Where(
                                      t => t is
                                          {
                                              IsAbstract: false,
                                              IsClass: true,
                                          }
                                          && typeof(ISyncProviderConfigurationRegistration).IsAssignableFrom(t)))
                .ToArray();

        foreach (Type type in implementedProviderRegistrations)
        {
            ISyncProviderConfigurationRegistration registry =
                (ISyncProviderConfigurationRegistration)Activator.CreateInstance(type);

            registry?.AddConfigurationDependencies(serviceCollection, systemConfigurationProviders, logger);
        }

        return logger.ExitMethod(serviceCollection);
    }
}
