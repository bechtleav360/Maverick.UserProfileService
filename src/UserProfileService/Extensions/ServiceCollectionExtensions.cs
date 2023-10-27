using Microsoft.Extensions.Options;
using UserProfileService.Configuration;
using UserProfileService.Proxy.Sync.Abstractions;
using UserProfileService.Services;
using UserProfileService.Utilities;

namespace UserProfileService.Extensions;

/// <summary>
///     Contains extensions of <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add all necessary components for internal requests to UPS-Sync
    /// </summary>
    /// <param name="services">The service collection <see cref="IServiceCollection" />.</param>
    /// <param name="optionsAction">The action <see cref="Action" /> used to configure the UPS-Sync endpoint.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"> will be thrown when the optionsAction is not set (null)</exception>
    public static IServiceCollection AddSyncRequester(
        this IServiceCollection services,
        Action<SyncOptions> optionsAction)
    {
        if (optionsAction == null)
        {
            throw new ArgumentNullException(nameof(optionsAction));
        }

        services.AddOptions<SyncOptions>().Configure(optionsAction);

        services.AddHttpClient(
            SyncConstants.SyncClient,
            (provider, httpClient) =>
            {
                using IServiceScope scope = provider.CreateScope();

                httpClient.BaseAddress = new Uri(
                    scope.ServiceProvider
                        .GetRequiredService<
                            IOptionsSnapshot<
                                SyncOptions>>()
                        .Value.Endpoint);
            });

        services.AddTransient<ISynchronizationService, ApiSynchronizationService>();
        services.AddTransient<IScheduleService, ApiScheduleService>();

        return services;
    }
}
