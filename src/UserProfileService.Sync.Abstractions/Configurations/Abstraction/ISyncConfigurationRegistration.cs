using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UserProfileService.Sync.Abstraction.Configurations.Abstraction;

/// <summary>
/// Contains methods to dependencies of search providers to a <see cref="IServiceCollection" />.
/// </summary>
public interface ISyncProviderConfigurationRegistration
{
    /// <summary>
    ///  Registers all important configuration dependencies to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="servicesCollection">The service collection that should contain the configuration dependencies.</param>
    /// <param name="providerConfiguration">The specific configuration of a provider.</param>
    /// <param name="logger">The logger is used for logging services.</param>
    void AddConfigurationDependencies(
        IServiceCollection servicesCollection,
        Dictionary<string, IConfigurationSection> providerConfiguration,
        ILogger logger);
}
