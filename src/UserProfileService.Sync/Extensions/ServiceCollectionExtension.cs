using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Factories;
using UserProfileService.Sync.Handlers;
using UserProfileService.Sync.Services.Comparer;
using UserProfileService.Sync.Systems;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Contains methods to extend <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Registers the configured implementation of <see cref="ISagaEntityProcessorFactory{TEntity}" /> for all entities.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSagaEntityProcessorFactories(this IServiceCollection services)
    {
        services.AddTransient<ISagaEntityProcessorFactory<UserSync>, DefaultSagaEntityProcessorFactory<UserSync>>();

        services
            .AddTransient<ISagaEntityProcessorFactory<GroupSync>, DefaultSagaEntityProcessorFactory<GroupSync>>();

        services
            .AddTransient<ISagaEntityProcessorFactory<OrganizationSync>,
                DefaultSagaEntityProcessorFactory<OrganizationSync>>();

        services.AddTransient<ISagaEntityProcessorFactory<RoleSync>, DefaultSagaEntityProcessorFactory<RoleSync>>();

        return services;
    }

    /// <summary>
    ///     Registers the configured implementation of <see cref="ISynchronizationWriteDestination{TEntity}" /> and
    ///     <see cref="ISynchronizationReadDestination{TEntity}" /> for all entities.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMaverickDestinationSystems(this IServiceCollection services)
    {
        services.AddScoped<ISynchronizationWriteDestination<GroupSync>, MaverickGroupDestinationSystem>();
        services.AddScoped<ISynchronizationReadDestination<GroupSync>, MaverickGroupDestinationSystem>();

        services.AddScoped<ISynchronizationWriteDestination<RoleSync>, MaverickRoleDestinationSystem>();
        services.AddScoped<ISynchronizationReadDestination<RoleSync>, MaverickRoleDestinationSystem>();

        services.AddScoped<ISynchronizationWriteDestination<UserSync>, MaverickUserDestinationSystem>();
        services.AddScoped<ISynchronizationReadDestination<UserSync>, MaverickUserDestinationSystem>();

        services.AddScoped<ISynchronizationWriteDestination<FunctionSync>, MaverickFunctionDestinationSystem>();
        services.AddScoped<ISynchronizationReadDestination<FunctionSync>, MaverickFunctionDestinationSystem>();

        services
            .AddScoped<ISynchronizationWriteDestination<OrganizationSync>, MaverickOrganizationDestinationSystem>();

        services
            .AddScoped<ISynchronizationReadDestination<OrganizationSync>, MaverickOrganizationDestinationSystem>();

        return services;
    }

    /// <summary>
    ///     Registers the needed comparer so that the sync can compare existing entity with the
    ///     one sync to. So the sync can decide if an entity has to be updated.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddModelComparer(this IServiceCollection services)
    {
        services.TryAddScoped<ISyncModelComparer<FunctionSync>, FunctionSyncComparer>();
        services.TryAddScoped<ISyncModelComparer<GroupSync>, GroupSyncComparer>();
        services.TryAddScoped<ISyncModelComparer<OrganizationSync>, OrganizationSyncComparer>();
        services.TryAddScoped<ISyncModelComparer<UserSync>, UserSyncComparer>();
        services.TryAddScoped<ISyncModelComparer<RoleSync>, RoleSyncComparer>();

        return services;
    }

    /// <summary>
    ///     Register the none relation factory dependencies for the sync system.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddNoneRelationFactoryDependencies(this IServiceCollection services)
    {
        services.TryAddScoped<IRelationFactory, NoneRelationFactory>();
        services.TryAddScoped<IRelationHandler<NoneSyncModel>, NoneRelationHandler>();

        return services;
    }

    /// <summary>
    ///     Adds options with validation to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <typeparam name="TValidator">The type of options validator implementing <see cref="IValidateOptions{TOptions}"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configSection">The configuration section to bind options from.</param>
    /// <param name="validateOnStart">Flag indicating whether to validate options on startup.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configSection"/> is null.
    /// </exception>
    public static IServiceCollection AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfigurationSection configSection,
        bool validateOnStart = false)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>, new()
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configSection == null)
        {
            throw new ArgumentNullException(nameof(configSection));
        }

        if (validateOnStart)
        {
            services.AddOptions<TOptions>()
                .Bind(configSection)
                .Validate(
                    config =>
                    {
                        ValidateOptionsResult validateResult = new TValidator().Validate(string.Empty, config);

                        if (validateResult.Failed)
                        {
                            throw new OptionsValidationException(
                                nameof(TOptions),
                                typeof(TOptions),
                                validateResult.Failures);
                        }

                        return true;
                    })
                .ValidateOnStart();
        }
        else
        {
            services.TryAddTransient<IValidateOptions<TOptions>, TValidator>();
            services.AddOptions<TOptions>().Bind(configSection);
        }


        return services;
    }

    /// <summary>
    ///     Adds options with validation to the <see cref="IServiceCollection"/> from a specified configuration section.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <typeparam name="TValidator">The type of options validator implementing <see cref="IValidateOptions{TOptions}"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options to.</param>
    /// <param name="configuration">The configuration from which the section will be retrieved.</param>
    /// <param name="configSectionName">The name of the configuration section to bind options from.</param>
    /// <param name="validateOnStart">Flag indicating whether to validate options on startup.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/>, <paramref name="configuration"/>, or <paramref name="configSectionName"/> is null.
    /// </exception>
    public static IServiceCollection AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName,
        bool validateOnStart = false)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>, new()
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configSectionName))
        {
            throw new ArgumentException("The section name should not be null, empty or whitespace");
        }

        return AddValidatedOptions<TOptions, TValidator>(
            services,
            configuration.GetSection(configSectionName),
            validateOnStart);

    }
}
