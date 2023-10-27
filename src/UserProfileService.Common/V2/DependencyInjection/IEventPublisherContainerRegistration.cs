using System;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.DependencyInjection;

/// <summary>
///     Represents a registration helper to register event publisher to a DI container.
/// </summary>
public interface IEventPublisherContainerRegistration
{
    /// <summary>
    ///     Adds an event publisher.
    /// </summary>
    /// <typeparam name="TEventPublisher">Type of the <see cref="IEventPublisher" />.</typeparam>
    /// <param name="resolver">The type resolving generation method.</param>
    /// <returns>A modified registration to be used in the setup process.</returns>
    IEventPublisherContainerRegistration AddEventPublisher<TEventPublisher>(
        Func<IServiceProvider, IEventPublisherTypeResolver> resolver = null)
        where TEventPublisher : class, IEventPublisher;
}
