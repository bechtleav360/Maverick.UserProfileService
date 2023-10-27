using UserProfileService.Common.V2.DependencyInjection;
using UserProfileService.EventSourcing.Abstractions.BasicImplementations;

namespace UserProfileService.EventSourcing.Abstractions.DependencyInjection;

/// <summary>
///     Contains extension methods to register event publisher to a DI container in a fluent way.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the default event publisher.
    /// </summary>
    /// <param name="registration">The registration helper to be used.</param>
    /// <returns>A modified registration builder instance.</returns>
    public static IEventPublisherContainerRegistration AddDefaultEventPublisher(
        this IEventPublisherContainerRegistration registration)
    {
        registration.AddEventPublisher<EventStoreEventPublisher>();

        return registration;
    }
}
