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
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using ServiceCollectionExtensions =
    UserProfileService.Marten.EventStore.DependencyInjection.ServiceCollectionExtensions;

namespace UserProfileService.Projection.SecondLevel.Assignments.DependencyInjection;

/// <summary>
///     Extension to register all dependencies fot the second level projection.
/// </summary>
public static class SecondLevelAssignmentProjectionBuilderExtension
{
    /// <summary>
    ///     Register the activity source wrapper to start activity tracing.
    /// </summary>
    /// <param name="builder">The builder that is used to configure the second level projection.</param>
    /// <param name="activitySource">The activity source that is used to extend the activity tracing.</param>
    /// <returns>
    ///     The <inheritdoc cref="ISecondLevelAssignmentProjectionBuilder" /> to create a fluent configuration
    ///     registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelAssignmentProjectionBuilder AddActivitySource(
        this ISecondLevelAssignmentProjectionBuilder builder,
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
    /// <returns>
    ///     The <inheritdoc cref="ISecondLevelAssignmentProjectionBuilder" /> to create a fluent configuration
    ///     registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelAssignmentProjectionBuilder AddActivitySourceDefault(
        this ISecondLevelAssignmentProjectionBuilder builder)
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
    /// <returns>
    ///     The <inheritdoc cref="ISecondLevelAssignmentProjectionBuilder" /> to create a fluent configuration
    ///     registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown when an method argument is null.</exception>
    public static ISecondLevelAssignmentProjectionBuilder AddHealthCheckStore(
        this ISecondLevelAssignmentProjectionBuilder builder)
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
    public static ISecondLevelAssignmentProjectionBuilder AddSecondLevelMapper(
        this ISecondLevelAssignmentProjectionBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

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
    public static ISecondLevelAssignmentProjectionBuilder AddSecondLevelStreamNameResolver(
        this ISecondLevelAssignmentProjectionBuilder builder,
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
    public static ISecondLevelAssignmentProjectionBuilder AddSecondLevelEventHandlers(
        this ISecondLevelAssignmentProjectionBuilder builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        foreach ((Type instanceType, Type serviceType) in
                 typeof(ISecondLevelAssignmentEventHandler<>).GetInstanceTypesForDependencyInjection(
                     typeof(SecondLevelAssignmentEventHandlerBase<>)
                         .Assembly))
        {
            builder.ServiceCollection.Add(ServiceDescriptor.Describe(serviceType, instanceType, serviceLifetime));
        }

        return builder;
    }
}
