using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UserProfileService.Hosting.Exceptions;

namespace UserProfileService.Hosting;

/// <summary>
///     Extensions for configuring host-builders and closely related components.
/// </summary>
public static class ApplicationBuilderExtensions
{
    private static void ConfigureDiscardResponsePathBase(IApplicationBuilder app, string discardResponsePathBase)
    {
        if (discardResponsePathBase != string.Empty)
        {
            if (!discardResponsePathBase.StartsWith('/'))
            {
                throw new InvalidRoutingConfigurationException("Routing:DiscardResponsePathBase must start with '/'");
            }

            app.Use(
                (context, next) =>
                {
                    if (context.Request.Path.StartsWithSegments(discardResponsePathBase, out PathString remainder))
                    {
                        context.Request.Path = remainder;
                    }

                    return next();
                });
        }
    }

    private static void ConfigurePathBase(IApplicationBuilder app, string pathBase)
    {
        if (pathBase != string.Empty)
        {
            if (!pathBase.StartsWith('/'))
            {
                throw new InvalidRoutingConfigurationException("Routing:PathBase must start with '/'");
            }

            if (pathBase.Contains("//"))
            {
                throw new InvalidRoutingConfigurationException(
                    "Routing:PathBase must not contain double-slashes ('//')");
            }

            try
            {
                app.UsePathBase(pathBase);
            }
            catch (Exception e)
            {
                throw new InvalidRoutingConfigurationException("Routing:PathBase is invalid, see inner-exception", e);
            }

            // rename variable for increased clarity
            // see readme for how this works and why we do this
            string responsePrefix = pathBase;

            app.Use(
                (context, next) =>
                {
                    context.Request.PathBase = new PathString(responsePrefix);

                    return next();
                });
        }
    }

    /// <summary>
    ///     Configure this app to work with different reverse-proxies, using the given configuration.
    /// </summary>
    /// <param name="app">the app to configure</param>
    /// <param name="configuration">the configuration containing the required config-sections ('Routing:*')</param>
    /// <returns>the given application-builder with added configuration.</returns>
    public static IApplicationBuilder UseReverseProxyPathBases(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        IConfigurationSection proxySection = configuration.GetSection("Routing");

        string pathBase = proxySection.GetValue<string>("PathBase") ?? string.Empty;
        string discardResponsePathBase = proxySection?.GetValue<string>("DiscardResponsePathBase") ?? string.Empty;

        if (pathBase != string.Empty && discardResponsePathBase != string.Empty)
        {
            throw new InvalidRoutingConfigurationException(
                "Routing:PathBase and Routing:DiscardResponsePathBase must not be used at the same time.");
        }

        ConfigurePathBase(app, pathBase);
        ConfigureDiscardResponsePathBase(app, discardResponsePathBase);

        return app;
    }
}
