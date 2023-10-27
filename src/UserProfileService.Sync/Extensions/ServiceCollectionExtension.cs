using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Factories;
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
}
