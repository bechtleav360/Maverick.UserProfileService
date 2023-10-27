using System;
using System.Diagnostics;
using System.Reflection;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Common;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Implementations;
using UserProfileService.Marten.EventStore.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Projection.Common.Services;
using UserProfileService.Projection.Common.Utilities;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Implementation;
using UserProfileService.Projection.FirstLevel.Utilities;

namespace UserProfileService.Projection.FirstLevel.DependencyInjection;

/// <summary>
///     Extension to register all dependencies fot the first level projection.
/// </summary>
public static class FirstLevelProjectionBuilderExtension
{
    /// <summary>
    ///     Register the activity source wrapper to start activity tracing.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <param name="activitySource">The activity source that is used to extend the activity tracing.</param>
    /// <returns>
    ///     The <inheritdoc cref="IFirstLevelProjectionBuilder" /> to
    ///     create a fluent configuration registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static IFirstLevelProjectionBuilder AddActivitySource(
        this IFirstLevelProjectionBuilder builder,
        ActivitySource activitySource)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (activitySource == null)
        {
            throw new ArgumentNullException(nameof(activitySource));
        }

        builder.ServiceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(IActivitySourceWrapper),
                p => new ActivitySourceWrapper(activitySource),
                ServiceLifetime.Singleton));

        return builder;
    }

    /// <summary>
    ///     Register the activity source wrapper default to start activity tracing.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <returns>
    ///     The <inheritdoc cref="IFirstLevelProjectionBuilder" /> to
    ///     create a fluent configuration registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static IFirstLevelProjectionBuilder AddActivitySourceDefault(this IFirstLevelProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(IActivitySourceWrapper),
                p => new ActivitySourceWrapper(
                    new ActivitySource(
                        Assembly.GetEntryAssembly()?.GetName().Name ?? "Default_ActivitySource",
                        Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0")),
                ServiceLifetime.Singleton));

        return builder;
    }

    /// <summary>
    ///     Register the health check store for the first level projection.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <returns>
    ///     The <inheritdoc cref="IFirstLevelProjectionBuilder" /> to
    ///     create a fluent configuration registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static IFirstLevelProjectionBuilder AddHealthCheckStore(this IFirstLevelProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.TryAddSingleton<ProjectionServiceHealthCheck>();

        return builder;
    }

    /// <summary>
    ///     Add for the first level projection the needed mapper.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <returns>The <see cref="IFirstLevelProjectionBuilder" /> itself.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static IFirstLevelProjectionBuilder AddFirstLevelMapper(
        this IFirstLevelProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.AddAutoMapper(
            typeof(FirstLevelProjectionMapper).Assembly,
            typeof(MappingProfiles).Assembly);

        return builder;
    }

    /// <summary>
    ///     Adds a tuple creator that is used  to create out of an <see cref="IUserProfileEvent" />
    ///     and a <see cref="ObjectIdent" /> an <see cref="EventTuple" /> for the <see cref="ISagaService" />.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <param name="serviceLifetime">
    ///     Specifies the lifetime of a service in an
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IFirstLevelProjectionBuilder AddFirstLevelTupleCreator(
        this IFirstLevelProjectionBuilder builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.Add(
            new ServiceDescriptor(
                typeof(IFirstLevelEventTupleCreator),
                typeof(FirstLevelEventCreator),
                serviceLifetime));

        return builder;
    }

    /// <summary>
    ///     Adds the stream name resolver. The resolver is used to resolve <see cref="ObjectIdent" />
    ///     to a stream name.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <param name="serviceLifetime">
    ///     Specifies the lifetime of a service in an
    ///     <see cref="IServiceCollection" />
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IFirstLevelProjectionBuilder AddFirstLevelStreamNameResolver(
        this IFirstLevelProjectionBuilder builder,
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
    ///     Adds the first level projection handlers to service collection, so that
    ///     it can be used to handle events of the first level projection.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <param name="serviceLifetime">
    ///     Specifies the lifetime of a service in an
    ///     <see cref="IServiceCollection" />.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    public static IFirstLevelProjectionBuilder AddFirstLevelEventHandlers(
        this IFirstLevelProjectionBuilder builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        foreach ((Type instanceType, Type serviceType) in
                 typeof(IFirstLevelProjectionEventHandler<>).GetInstanceTypesForDependencyInjection(
                     typeof(FirstLevelEventHandlerBase<>)
                         .Assembly))
        {
            builder.ServiceCollection.Add(ServiceDescriptor.Describe(serviceType, instanceType, serviceLifetime));
        }

        return builder;
    }

    /// <summary>
    ///     Adds the special resolver for the first level handler.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the first level projection.</param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    public static IFirstLevelProjectionBuilder AddHandlerResolver(
        this IFirstLevelProjectionBuilder builder)
    {
        builder
            .ServiceCollection
            .AddTransient<IPropertiesChangedRelatedEventsResolver, PropertiesChangedRelatedEventsResolver>();

        return builder;
    }
}
