using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Projection.Common.DependencyInjection;

/// <summary>
///     Defines an interface for building options related to saga services.
/// </summary>
public interface ISagaServiceOptionsBuilder
{
    /// <summary>
    ///     Gets the <see cref="IServiceCollection"/> associated with the options.
    /// </summary>
    IServiceCollection Services { get; }
}
