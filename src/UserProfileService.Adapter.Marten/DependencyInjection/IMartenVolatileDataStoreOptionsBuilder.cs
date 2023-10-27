using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Adapter.Marten.DependencyInjection;

/// <summary>
///     Defines a configuration builder for the volatile store using Marten.
/// </summary>
public interface IMartenVolatileDataStoreOptionsBuilder
{
    /// <summary>
    ///     The service collection containing all registered services.
    /// </summary>
    IServiceCollection Services { get; }
}
