using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Extension that is used to add headers to a service.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the forwarded header to the service.
    /// </summary>
    /// <param name="services">The service to register the header to.</param>
    /// <returns>Returns the <see cref="IServiceCollection" /> itself.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Will be thrown when the
    ///     <param name="services"> is null.</param>
    /// </exception>
    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Configure<ForwardedHeadersOptions>(
            options =>
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                options.ForwardedHeaders = ForwardedHeaders.All;
            });

        return services;
    }
}
