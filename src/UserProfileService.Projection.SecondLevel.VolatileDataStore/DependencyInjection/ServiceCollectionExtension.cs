using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.VolatileDataStore.Abstractions;
using UserProfileService.Projection.SecondLevel.VolatileDataStore.Services;

namespace UserProfileService.Projection.SecondLevel.VolatileDataStore.DependencyInjection;

/// <summary>
///     Contains methods to extend <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Register the second level projection service that projects the event
    ///     in the main stream.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="options">The options that can be configured for the second level projection.</param>
    /// <returns>An Instance of <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddVolatileDataProjectionService(
        this IServiceCollection services,
        Action<ISecondLevelVolatileDataProjectionBuilder> options
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

        var builder = new SecondLevelVolatileDataProjectionBuilder(
            services,
            typeof(VolatileDataSecondLevelProjectionService).FullName!);

        options.Invoke(builder);

        services.AddSingleton<ISecondLevelVolatileDataEventHandler, MainSecondLevelVolatileDataEventHandler>();
        services.AddSingleton<IVolatileDataSecondLevelProjection, VolatileDataSecondLevelProjectionService>();

        return services;
    }
}
