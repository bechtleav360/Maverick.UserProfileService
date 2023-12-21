using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;
using UserProfileService.Projection.SecondLevel.Services;

namespace UserProfileService.Projection.SecondLevel.DependencyInjection;

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
    public static IServiceCollection AddSecondLevelProjectionService(
        this IServiceCollection services,
        Action<ISecondLevelProjectionBuilder> options
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

        var builder = new SecondLevelProjectionBuilder(services, typeof(ApiSecondLevelProjectionService).FullName);

        options.Invoke(builder);

        services.TryAddSingleton<ISecondLevelEventHandler, MainSecondLevelEventHandler>();
        services.TryAddSingleton<ISecondLevelProjection, ApiSecondLevelProjectionService>();

        return services;
    }
}
