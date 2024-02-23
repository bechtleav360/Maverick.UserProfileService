using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Configurations.Abstraction;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Configuration;
using UserProfileService.Sync.Systems;
using UserProfileService.Sync.Validation;

namespace UserProfileService.Sync.ConfigurationProvider;

/// <summary>
///     Registers all dependencies for the LDAP-System.
/// </summary>
public class LdapConfigurationDependencyRegistration : DependencyRegistrationBase
{
    /// <inheritdoc />
    protected override void RegisterSpecificDependencies(
        IServiceCollection serviceCollection,
        ILogger logger,
        IConfigurationSection syncProviderConfigurationSection)
    {
        serviceCollection.AddValidatedOptions<LdapSystemConfiguration, LdapConfigurationValidation>(
            syncProviderConfigurationSection,
            true);
        serviceCollection.AddScoped<ISynchronizationSourceSystem<GroupSync>, LdapSourceSystem>();
        serviceCollection.AddScoped<ISynchronizationSourceSystem<UserSync>, LdapSourceSystem>();
    }

    /// <inheritdoc />
    protected override Type GetRelevantProviderType()
    {
        return typeof(LdapSystemConfiguration);
    }
}
