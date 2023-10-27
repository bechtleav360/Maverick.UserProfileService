using System;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.Services;

namespace UserProfileService.Sync.Projection.DependencyInjection;

/// <summary>
///     Contains methods to extend <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Register the first level projection service that projects the event
    ///     in the main stream.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="options">The options that can be configured for the first level projection.</param>
    /// <returns>An Instance of <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddSyncProjectionService(
        this IServiceCollection services,
        Action<IProjectionBuilder> options
    )
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        if (options == null)
        {
            throw new ArgumentNullException($"The variable {nameof(options)} was null!");
        }

        var builder = new SyncProjectionBuilder(services, typeof(SyncProjectionService).FullName);

        options.Invoke(builder);

        services.AddSingleton<ISyncProjectionEventHandler, MainSyncEventHandler>();
        services.AddSingleton<ISyncProjection, SyncProjectionService>();

        return services;
    }

    /// <summary>
    ///     Register the second level projection service that projects the events of the second level streams.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <returns>An Instance of <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddSecondLevelSyncProjection(
        this IServiceCollection services
    )
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        services.AddSingleton<IProfileService, ProfileService>();

        return services;
    }
}
