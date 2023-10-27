using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Hosting.Abstraction;
using UserProfileService.Hosting.Tracing;

namespace UserProfileService.Hosting;

/// <summary>
///     UserProfileService specific helper to create a default <see cref="IHostBuilder" />
/// </summary>
public static class UseProfileServiceHostBuilder
{
    /// <summary>
    /// Configure the host with default and own configuration.
    /// </summary>
    /// <param name="args">The console arguments tha can be given to the host.</param>
    /// <returns>A <see cref="IHostBuilder"/> that can be started to run.</returns>
    public static IHostBuilder CreateDefaultBuilder<TStartUp>(
        string[] args) where TStartUp : DefaultStartupBase
    {
        // Why set a global timeout?
        // Regular expressions could be used by an attacker to launch a denial-of-service attack for a website
        // by consuming excessive resources. Setting a timeout allows the operation to stop at a configured timeout,
        // rather than running until completion, using resources the entire time.
        // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matchtimeout?view=net-6.0#remarks
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));

        var hostBuilder = new HostBuilder();
        
        hostBuilder.UseContentRoot(Directory.GetCurrentDirectory());

        hostBuilder.ConfigureHostConfiguration(
                (config) =>
                {
                    config.AddEnvironmentVariables("DOTNET_");
                    config.AddEnvironmentVariables("ASPNETCORE_");
                    config.AddJsonFile("appsettings.json", true, true);
                    config.AddCommandLine(args);
                })
            .ConfigureAppConfiguration(
                (context, builder) =>
                {
                    builder.AddEnvironmentVariables("DOTNET_");
                    builder.AddEnvironmentVariables("ASPNETCORE_");
                    builder.AddJsonFile("appsettings.json", true, true);
                    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
                    builder.AddCommandLine(args);
                })
            .UseDefaultServiceProvider(
                (context, options) =>
                {
                    bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDevelopment;
                    options.ValidateOnBuild = isDevelopment;
                })
            .ConfigureLogging(
                (context, loggingBuilder) => { loggingBuilder.UseSpecificLogging(context.Configuration); })
            .ConfigureServices(
                (hostBuilderContext, serviceCollection) =>
                {
                    // Add Tracing via OpenTelemetry
                    var tracingOptions = hostBuilderContext.Configuration?.GetSection("Tracing")?.Get<TracingOptions>();

                    if (tracingOptions == null)
                    {
                        return;
                    }

                    serviceCollection.AddUserProfileServiceTracing(
                        options =>
                        {
                            options.ServiceName = tracingOptions.ServiceName;
                            options.OtlpEndpoint = tracingOptions.OtlpEndpoint;
                        });

                    serviceCollection.AddForwardedHeaders();
                })
            .ConfigureWebHostDefaults(host => host.UseStartup<TStartUp>());

        return hostBuilder;
    }
}
