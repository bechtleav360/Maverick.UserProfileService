using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.FirstLevel.DependencyInjection;

/// <summary>
///     This builder is used to build the first level projection
///     and all the dependencies.
/// </summary>
internal class FirstLevelProjectionBuilder : IFirstLevelProjectionBuilder
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
    ///     Creates an instance of the object <see cref="FirstLevelProjectionBuilder" />
    /// </summary>
    /// <param name="serviceCollection">The service collection that is used to register services.</param>
    /// <param name="name">The name of the projections.</param>
    public FirstLevelProjectionBuilder(IServiceCollection serviceCollection, string name)
    {
        ServiceCollection = serviceCollection;
        Name = name;
    }
}
