using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Projection.SecondLevel.VolatileDataStore.DependencyInjection;

internal class SecondLevelVolatileDataProjectionBuilder : ISecondLevelVolatileDataProjectionBuilder
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
    ///     Creates an instance of the object <see cref="SecondLevelVolatileDataProjectionBuilder" />
    /// </summary>
    /// <param name="serviceCollection">The service collection that is used to register services.</param>
    /// <param name="name">The name of the projections.</param>
    public SecondLevelVolatileDataProjectionBuilder(
        IServiceCollection serviceCollection,
        string name)
    {
        ServiceCollection = serviceCollection;
        Name = name;
    }
}
