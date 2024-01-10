using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Configurations.Abstraction;
using UserProfileService.Sync.Configuration;

namespace UserProfileService.Sync.ConfigurationProvider;

/// <summary>
/// 
/// </summary>
public class LdapConfigurationDependencyRegistration: DependencyRegistrationBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configurationSection"></param>
    public LdapConfigurationDependencyRegistration(string configurationSection) : base(configurationSection)
    {
    }

    protected override void RegisterSpecificDependencies(
        IServiceCollection serviceCollection,
        ILogger logger,
        IConfigurationSection syncProviderConfigurationSection)
    {
        serviceCollection.Configure<LdapSystemConfiguration>(syncProviderConfigurationSection);
    }

    /// <inheritdoc/>
    protected override Type GetRelevantProviderType()
    {
        return typeof(LdapSystemConfiguration);
    }
}
