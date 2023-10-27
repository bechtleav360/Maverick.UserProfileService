using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Projection.Common.DependencyInjection;

public interface ISagaServiceOptionsBuilder
{
    IServiceCollection Services { get; }
}
