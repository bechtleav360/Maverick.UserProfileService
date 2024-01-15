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
/// 
/// </summary>
public static class SyncConfigurationExtensions
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="assemblies"></param>
    /// <param name="mainConfiguration"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
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

        var configurationToBeUser = assemblies
                                    .SelectMany(a => a.GetTypes())
                                    .Where(
                                        p => !string.IsNullOrEmpty(
                                            p.GetCustomAttribute<SynchConfigurationProviderAttribute>()
                                             ?.SyncConfigName))
                                    .Select(
                                        p => p.GetCustomAttribute<SynchConfigurationProviderAttribute>().SyncConfigName)
                                    .ToList();

        var systemConfigurationProviders = mainConfiguration
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
            var registry = (ISyncProviderConfigurationRegistration?)
                Activator.CreateInstance(type);

            registry?.AddConfigurationDependencies(serviceCollection, systemConfigurationProviders, logger);
        }

        return logger.ExitMethod(serviceCollection);
    }
}
