using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Services;

namespace UserProfileService.Projection.Common.DependencyInjection;

/// <summary>
///     Contains methods to register projection common services to DI service container.
/// </summary>
public static class ProjectionCommonsServiceCollectionExtensions
{
    /// <summary>
    ///     Attempts to add the default implementation of <see cref="IProjectionResponseService" /> to a
    ///     <paramref name="serviceCollection" />.
    /// </summary>
    /// <param name="serviceCollection">The service collection to be add to</param>
    /// <param name="serviceLifetime">The lifetime of the implementation to be added.</param>
    public static void AddProjectionResponseService(
        this IServiceCollection serviceCollection,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        serviceCollection.TryAdd(
            ServiceDescriptor.Describe(
                typeof(IProjectionResponseService),
                typeof(ProjectionSagaResponseService),
                serviceLifetime));
    }
}
