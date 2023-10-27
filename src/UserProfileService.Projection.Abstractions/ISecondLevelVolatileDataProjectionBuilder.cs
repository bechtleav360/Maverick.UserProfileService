using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     The second level projection builder to register and configure the second level volatile data projection.
/// </summary>
public interface ISecondLevelVolatileDataProjectionBuilder
{
    /// <summary>
    ///     The name of the first level projection.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The service collection to register services.
    /// </summary>
    IServiceCollection ServiceCollection { get; }
}
