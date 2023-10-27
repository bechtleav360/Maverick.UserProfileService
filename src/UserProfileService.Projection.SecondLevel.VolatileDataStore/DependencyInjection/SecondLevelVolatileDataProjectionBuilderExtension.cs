using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Marten.EventStore.DependencyInjection;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Projection.Common.Services;
using UserProfileService.Projection.SecondLevel.VolatileDataStore.Abstractions;

namespace UserProfileService.Projection.SecondLevel.VolatileDataStore.DependencyInjection;

/// <summary>
///     Extension to register all dependencies fot the second level projection.
/// </summary>
public static class SecondLevelVolatileDataProjectionBuilderExtension
{
    /// <summary>
    ///     Register the health check store for the second level projection.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <returns>
    ///     The <inheritdoc cref="ISecondLevelAssignmentProjectionBuilder" /> to create a fluent configuration
    ///     registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelVolatileDataProjectionBuilder AddHealthCheckStore(
        this ISecondLevelVolatileDataProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.TryAddSingleton<ProjectionServiceHealthCheck>();

        return builder;
    }

    /// <summary>
    ///     Adds the stream name resolver. The resolver is used to resolve <see cref="ObjectIdent" />
    ///     to a stream name.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <param name="serviceLifetime">
    ///     Specifies the lifetime of a service in an
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ISecondLevelVolatileDataProjectionBuilder AddSecondLevelStreamNameResolver(
        this ISecondLevelVolatileDataProjectionBuilder builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.TryAddStreamNameResolved(serviceLifetime);

        return builder;
    }

    /// <summary>
    ///     Adds the second level projection handlers to service collection, so that
    ///     it can be used to handle events of the second level projection.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <param name="serviceLifetime">
    ///     Specifies the lifetime of a service in an
    ///     <see cref="IServiceCollection" />.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    public static ISecondLevelVolatileDataProjectionBuilder AddSecondLevelEventHandlers(
        this ISecondLevelVolatileDataProjectionBuilder builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        foreach ((Type instanceType, Type serviceType) in
                 typeof(ISecondLevelVolatileDataEventHandler<>).GetInstanceTypesForDependencyInjection(
                     typeof(SecondLevelVolatileDataEventHandlerBase<>)
                         .Assembly))
        {
            builder.ServiceCollection.Add(ServiceDescriptor.Describe(serviceType, instanceType, serviceLifetime));
        }

        return builder;
    }
}
