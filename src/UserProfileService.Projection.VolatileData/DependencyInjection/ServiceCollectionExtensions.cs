using System;
using UserProfileService.Common.V2.DependencyInjection;

namespace UserProfileService.Projection.VolatileData.DependencyInjection;

/// <summary>
///     Contains extension methods to register volatile data stores to DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add a volatile data event publisher.
    /// </summary>
    /// <param name="registration">The registration helper instance.</param>
    /// <param name="setup">The setup function.</param>
    /// <returns>A modified registration helper instance.</returns>
    public static IEventPublisherContainerRegistration AddVolatileDataEventPublisher(
        this IEventPublisherContainerRegistration registration,
        Action<IVolatileDataStoreConfigBuilder> setup)
    {
        var setupState = new VolatileDataStoreConfigBuilder();
        setup.Invoke(setupState);

        registration.AddEventPublisher<VolatileDataDefaultEventPublisher>(
            p => new VolatileDataStorePublisherResolver(p, setupState.SupportedTypes));

        return registration;
    }
}
