using System;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Services;

namespace UserProfileService.Projection.SecondLevel.Assignments.DependencyInjection;

/// <summary>
///     Contains methods to extend <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Register the second level projection service that projects the event
    ///     in the main stream.
    /// </summary>
    /// <param name="services">The collection of services that is used for registration.</param>
    /// <param name="options">The options that can be configured for the second level projection.</param>
    /// <returns>An Instance of <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddAssignmentProjectionService(
        this IServiceCollection services,
        Action<ISecondLevelAssignmentProjectionBuilder> options
    )
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        if (options == null)
        {
            throw new ArgumentNullException($"The variable {nameof(options)} was null!");
        }

        var builder = new SecondLevelAssignmentProjectionBuilder(
            services,
            typeof(AssignmentsSecondLevelProjectionService).FullName);

        options.Invoke(builder);

        services.AddSingleton<ISecondLevelAssignmentEventHandler, MainSecondLevelAssignmentEventHandler>();
        services.AddSingleton<IAssignmentsSecondLevelProjection, AssignmentsSecondLevelProjectionService>();

        return services;
    }
}
