using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.DependencyInjection;
using UserProfileService.Saga.Common.Implementations;

namespace UserProfileService.Saga.Common.DependencyInjection;

/// <summary>
///     Contains extension methods to register <see cref="IEventPublisherFactory" /> to an
///     <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the event publisher dependencies that will take care of incoming events (either EventStore or custom store
    ///     handlers)
    /// </summary>
    /// <param name="services">The service collection the event publisher dependencies should be added to.</param>
    /// <param name="setup">A configuration method that will invoked to setup event publishers.</param>
    public static void AddEventPublisherDependencies(
        this IServiceCollection services,
        Action<IEventPublisherContainerRegistration> setup)
    {
        var options = new InternalEventPublisherContainerRegistration(services);
        setup.Invoke(options);

        services.TryAddSingleton<IEventPublisherFactory, DefaultEventPublisherFactory>();
        services.TryAddSingleton<EventProcessingSetup>(); // fallback
    }
}
