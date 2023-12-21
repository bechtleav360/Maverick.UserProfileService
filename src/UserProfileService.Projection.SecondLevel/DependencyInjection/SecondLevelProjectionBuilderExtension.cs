using System;
using System.Diagnostics;
using System.Reflection;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Implementations;
using UserProfileService.Marten.EventStore.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Projection.Common.Services;
using UserProfileService.Projection.SecondLevel.Abstractions;
using UserProfileService.Projection.SecondLevel.Utilities;
using MappingProfilesCommon = UserProfileService.Projection.Common.Utilities.MappingProfiles;

namespace UserProfileService.Projection.SecondLevel.DependencyInjection;

/// <summary>
///     Extension to register all dependencies fot the second level projection.
/// </summary>
public static class SecondLevelProjectionBuilderExtension
{
    /// <summary>
    ///     Register the activity source wrapper to start activity tracing.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <param name="activitySource">The activity source that is used to extend the activity tracing.</param>
    /// <returns>The <inheritdoc cref="ISecondLevelProjectionBuilder" /> to create a fluent configuration registration.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelProjectionBuilder AddActivitySource(
        this ISecondLevelProjectionBuilder builder,
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
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <returns>The <inheritdoc cref="ISecondLevelProjectionBuilder" /> to create a fluent configuration registration.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelProjectionBuilder AddActivitySourceDefault(this ISecondLevelProjectionBuilder builder)
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
    ///     Register the health check store for the second level projection.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <returns>The <inheritdoc cref="ISecondLevelProjectionBuilder" /> to create a fluent configuration registration.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelProjectionBuilder AddHealthCheckStore(this ISecondLevelProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.TryAddSingleton<ProjectionServiceHealthCheck>();

        return builder;
    }

    /// <summary>
    ///     Add for the second level projection the needed mapper.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <returns>The <see cref="IServiceCollection" /> itself.</returns>
    public static ISecondLevelProjectionBuilder AddSecondLevelMapper(
        this ISecondLevelProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.AddAutoMapper(
            typeof(MappingProfiles).Assembly,
            typeof(MappingProfilesCommon).Assembly);

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
    public static ISecondLevelProjectionBuilder AddSecondLevelStreamNameResolver(
        this ISecondLevelProjectionBuilder builder,
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
    ///     Adds second level projection event handlers to the specified <see cref="ISecondLevelProjectionBuilder" />.
    ///     These handlers can be used to process events related to the second level projection.
    /// </summary>
    /// <param name="builder">The builder used to configure the second level projection.</param>
    /// <param name="assembly">
    ///     An optional assembly to scan for second level projection event handler types.
    ///     If not provided, the assembly of the generic base handler, <see cref="SecondLevelEventHandlerBase{T}" />,
    ///     will be used by default.
    /// </param>
    /// <param name="serviceLifetime">
    ///     Specifies the lifetime of the added services in the <see cref="IServiceCollection" />.
    /// </param>
    /// <returns>
    ///     The <see cref="ISecondLevelProjectionBuilder" /> itself after adding the event handler services
    ///     to the service collection.
    /// </returns>
    public static ISecondLevelProjectionBuilder AddSecondLevelEventHandlers(
        this ISecondLevelProjectionBuilder builder,
        Assembly assembly = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        foreach ((Type instanceType, Type serviceType) in
                 typeof(ISecondLevelEventHandler<>).GetInstanceTypesForDependencyInjection(
                     assembly
                     ?? typeof(SecondLevelEventHandlerBase<>)
                         .Assembly))
        {
            builder.ServiceCollection.TryAdd(ServiceDescriptor.Describe(serviceType, instanceType, serviceLifetime));
        }

        return builder;
    }
}
