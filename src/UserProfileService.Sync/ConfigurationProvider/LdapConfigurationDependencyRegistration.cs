using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Configurations.Abstraction;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Configuration;
using UserProfileService.Sync.Systems;

namespace UserProfileService.Sync.ConfigurationProvider;

/// <summary>
/// 
/// </summary>
public class LdapConfigurationDependencyRegistration: DependencyRegistrationBase
{
    /// <inheritdoc/>
    protected override void RegisterSpecificDependencies(
        IServiceCollection serviceCollection,
        ILogger logger,
        IConfigurationSection syncProviderConfigurationSection)
    {
        serviceCollection.Configure<LdapSystemConfiguration>(syncProviderConfigurationSection);
        serviceCollection.AddScoped<ISynchronizationSourceSystem<GroupSync>,LdapSourceSystem>();
        serviceCollection.AddScoped<ISynchronizationSourceSystem<UserSync>, LdapSourceSystem>();
    }

    /// <inheritdoc/>
    protected override Type GetRelevantProviderType()
    {
        return typeof(LdapSystemConfiguration);
    }
}
