using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Services;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.DependencyInjection;
using UserProfileService.Projection.FirstLevel.Implementation;
using UserProfileService.Projection.FirstLevel.Services;

namespace UserProfileService.Projection.FirstLevel.Extensions;

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
    public static IServiceCollection AddFirstLevelProjectionService(
        this IServiceCollection services,
        Action<IFirstLevelProjectionBuilder> options
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

        var builder = new FirstLevelProjectionBuilder(services, typeof(FirstLevelProjectionService).FullName);

        options.Invoke(builder);

        services.TryAddSingleton<IFirstLevelProjectionEventHandler, MainFirstEventHandler>();

        services.TryAddScoped<ITemporaryAssignmentsExecutor, TemporaryAssignmentsExecutor>();
        services.TryAddScoped<ICronJobService, FirstLevelProjectionCronJobService>();

        services.TryAddSingleton<IFirstLevelProjection, FirstLevelProjectionService>();
        services.AddHostedService<CronJobServiceManager>();

        return services;
    }
}
