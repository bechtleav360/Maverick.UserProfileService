using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Projection.Common.Abstractions;

/// <summary>
///     The first level projection builder to register and configure the first level projection.
/// </summary>
public interface IProjectionBuilder
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
