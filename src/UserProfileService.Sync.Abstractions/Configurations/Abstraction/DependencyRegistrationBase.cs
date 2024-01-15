using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Annotations;

namespace UserProfileService.Sync.Abstraction.Configurations.Abstraction;

/// <summary>
///     Base class to help registering search dependency in DI container.
/// </summary>
public abstract class DependencyRegistrationBase : ISyncProviderConfigurationRegistration
{
    /// <summary>
    ///     Initializes a new instance of <see cref="DependencyRegistrationBase" /> with a specified key of a configuration
    ///     section.
    /// </summary>
    /// <remarks>
    ///     The constructor should be used by derived types only.
    /// </remarks>
    protected DependencyRegistrationBase()
    {
    }
    

    /// <summary>
    ///     Registers all provider specific dependencies in the <paramref name="serviceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The service collection the dependencies should be added to.</param>
    /// <param name="logger">The logger that takes log messages of this method.</param>
    /// <param name="searchProviderConfigurationSection">The configuration section that contains provider specific settings.</param>
    protected abstract void RegisterSpecificDependencies(
        IServiceCollection serviceCollection,
        ILogger logger,
        IConfigurationSection searchProviderConfigurationSection);

    /// <summary>
    ///     Returns the type of the sync provider that should be used for naming / setting up all dependencies.<br />
    ///     The regarding class must contain the <see cref="SynchConfigurationProviderAttribute" />, otherwise this registrations
    ///     will be skipped.
    /// </summary>
    /// <returns>The type of the search provider.</returns>
    protected abstract Type GetRelevantProviderType();

    /// <inheritdoc />
    public void AddConfigurationDependencies(
        IServiceCollection servicesCollection,
        Dictionary<string, IConfigurationSection> providerConfiguration,
        ILogger logger)

    {
        var providerMetadata = GetRelevantProviderType().GetCustomAttribute<SynchConfigurationProviderAttribute>();

        if (providerMetadata?.SyncConfigName == null
            || !providerConfiguration.TryGetValue(
                providerMetadata.SyncConfigName,
                out IConfigurationSection? searchProviderConfigurationSection))
        {
            logger.LogWarnMessage(
                "The sync configuration provider with the name {configProviderName} won't be registered.",
                LogHelpers.Arguments(providerMetadata.SyncConfigName));

            return;
        }

        RegisterSpecificDependencies(servicesCollection, logger, searchProviderConfigurationSection);
    }
}
