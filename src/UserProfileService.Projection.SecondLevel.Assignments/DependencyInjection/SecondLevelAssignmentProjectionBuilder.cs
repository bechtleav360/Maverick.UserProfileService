using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.DependencyInjection;

internal class SecondLevelAssignmentProjectionBuilder : ISecondLevelAssignmentProjectionBuilder
{
    /// <summary>
    ///     The name of the projections.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The service collection that is used to register services.
    /// </summary>
    public IServiceCollection ServiceCollection { get; }

    /// <summary>
    ///     Creates an instance of the object <see cref="SecondLevelAssignmentProjectionBuilder" />
    /// </summary>
    /// <param name="serviceCollection">The service collection that is used to register services.</param>
    /// <param name="name">The name of the projections.</param>
    public SecondLevelAssignmentProjectionBuilder(IServiceCollection serviceCollection, string name)
    {
        ServiceCollection = serviceCollection;
        Name = name;
    }
}
