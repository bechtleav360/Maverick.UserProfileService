using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Implementations;

namespace UserProfileService.Projection.Common.DependencyInjection;

/// <summary>
///     Extension class containing extension methods for <see cref="IServiceCollection"/>s that add
///     services for the first level projection.
/// </summary>
public static class FirstLevelServiceCollectionsExtensions
{
    private static IServiceCollection AddOutboxProcessorService(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IOutboxProcessorService, MartenEventStoreOutboxProcessorService>();

        return services;
    }

    private static IServiceCollection AddSagaService(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddTransient<ISagaService, MartenEventStoreSagaService>();
        services.AddHostedService<OutboxWorkerProcess>();

        return services;
    }

    // TODO: Validate if all dependencies are there, after
    // TODO: joining all components together.
    /// <summary>
    ///     Adds all the dependencies for the <see cref="ISagaService" />.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="eventLogWriter"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IFirstLevelProjectionBuilder AddFirstLevelSagaService(
        this IFirstLevelProjectionBuilder builder,
        Action<ISagaServiceOptionsBuilder> eventLogWriter,
        ILogger logger = null)
    {
        IServiceCollection services = builder?.ServiceCollection;

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (eventLogWriter == null)
        {
            throw new ArgumentNullException(nameof(eventLogWriter));
        }

        var optionsBuilder = new SagaServiceOptionsBuilder(services);
        eventLogWriter.Invoke(optionsBuilder);

        services
            .AddOutboxProcessorService()
            .AddSagaService()
            .AddHostedService<OutboxWorkerProcess>();

        return builder;
    }
}
