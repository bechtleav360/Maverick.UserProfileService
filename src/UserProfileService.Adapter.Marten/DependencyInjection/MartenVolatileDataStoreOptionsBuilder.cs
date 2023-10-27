using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Adapter.Marten.DependencyInjection;

internal class MartenVolatileDataStoreOptionsBuilder : IMartenVolatileDataStoreOptionsBuilder
{
    public IServiceCollection Services { get; }

    public MartenVolatileDataStoreOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
