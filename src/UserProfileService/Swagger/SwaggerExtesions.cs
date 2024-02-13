using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace UserProfileService.Swagger;

/// <summary>
///     Extensions to configure Swagger for the
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    ///     Add Swagger with default-values to the given AppBuilder
    /// </summary>
    /// <param name="builder">ApplicationBuilder to configure</param>
    /// <param name="useDefaultConfig">flag indicating if default-config should be done or not</param>
    /// <param name="setupSwagger">custom-config to execute after default-config</param>
    /// <exception cref="ArgumentNullException">thrown when <paramref name="builder" /> is null</exception>
    /// <returns>configured instance of <paramref name="builder" /></returns>
    public static IApplicationBuilder UseMaverickSwaggerWithVersions(
        this IApplicationBuilder builder,
        bool useDefaultConfig = true,
        Action<SwaggerUIOptions> setupSwagger = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var provider = builder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        builder
            .UseSwagger()
            .UseSwaggerUI(
                options =>
                {
                    if (useDefaultConfig)
                    {
                        options.DefaultModelExpandDepth(0);
                        options.DefaultModelRendering(ModelRendering.Example);
                        options.DocExpansion(DocExpansion.None);
                        options.DisplayRequestDuration();
                        options.EnableDeepLinking();
                        options.EnableFilter();
                        options.ShowExtensions();

                        foreach (ApiVersionDescription description in
                                 provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
                        {
                            options.SwaggerEndpoint(
                                $"{description.GroupName}/swagger.json",
                                description.GroupName.ToUpperInvariant());
                        }
                    }

                    setupSwagger?.Invoke(options);
                });

        return builder;
    }

    /// <summary>
    ///     Add Swagger with default-values to the given Service-Collection
    /// </summary>
    /// <param name="services">Service-Collection to configure</param>
    /// <param name="useDefaultConfig">flag indicating if default-config should be done or not</param>
    /// <param name="setupSwagger">custom-config to execute after default-config</param>
    /// <typeparam name="TOptions">type implementing <see cref="IConfigureOptions{TOptions}" /></typeparam>
    /// <exception cref="ArgumentNullException">thrown when <paramref name="services" /> is null</exception>
    /// <returns>the configured <paramref name="services" /> instance</returns>
    public static IServiceCollection AddMaverickSwaggerWithVersions<TOptions>(
        this IServiceCollection services,
        bool useDefaultConfig = true,
        Action<SwaggerGenOptions> setupSwagger = null)
        where TOptions : class, IConfigureOptions<SwaggerGenOptions>
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (services.Any(s => s.ServiceType == typeof(IConfigureOptions<SwaggerGenOptions>)))
        {
            return services;
        }

        services.TryAddTransient<IConfigureOptions<SwaggerGenOptions>, TOptions>();

        services.AddSwaggerGen(
            options =>
            {
                if (useDefaultConfig)
                {
                    options.CustomSchemaIds(t => t.FullName);

                    var ass = Assembly.GetExecutingAssembly();
                    string docFile = Path.Combine(AppContext.BaseDirectory, $"{ass.GetName().Name}.xml");

                    if (File.Exists(docFile))
                    {
                        options.IncludeXmlComments(docFile);
                    }
                }

                setupSwagger?.Invoke(options);
            });

        return services;
    }
}
