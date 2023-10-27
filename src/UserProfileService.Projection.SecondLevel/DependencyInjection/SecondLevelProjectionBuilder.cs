using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.SecondLevel.DependencyInjection;

internal class SecondLevelProjectionBuilder : ISecondLevelProjectionBuilder
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
    ///     Creates an instance of the object <see cref="SecondLevelProjectionBuilder" />
    /// </summary>
    /// <param name="serviceCollection">The service collection that is used to register services.</param>
    /// <param name="name">The name of the projections.</param>
    public SecondLevelProjectionBuilder(IServiceCollection serviceCollection, string name)
    {
        ServiceCollection = serviceCollection;
        Name = name;
    }
}
