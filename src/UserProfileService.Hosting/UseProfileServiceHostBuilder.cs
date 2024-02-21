using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Hosting.Abstraction;

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
    public static IWebHostBuilder CreateDefaultBuilder<TStartUp>(
        string[] args) where TStartUp : DefaultStartupBase
    {
        // Why set a global timeout?
        // Regular expressions could be used by an attacker to launch a denial-of-service attack for a website
        // by consuming excessive resources. Setting a timeout allows the operation to stop at a configured timeout,
        // rather than running until completion, using resources the entire time.
        // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matchtimeout?view=net-6.0#remarks
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));

        var hostBuilder = WebHost.CreateDefaultBuilder<TStartUp>(args)

                                 .ConfigureLogging(
                                     (context, loggingBuilder) =>
                                     {
                                         loggingBuilder.UseSpecificLogging(context.Configuration);
                                     });


        return hostBuilder;
    }
}
