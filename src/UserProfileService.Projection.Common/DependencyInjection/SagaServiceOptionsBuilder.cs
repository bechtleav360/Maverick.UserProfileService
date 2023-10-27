using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Projection.Common.DependencyInjection;

internal class SagaServiceOptionsBuilder : ISagaServiceOptionsBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    internal SagaServiceOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
