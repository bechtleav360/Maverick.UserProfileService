using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Defines a startup as a fallback, that won't use any configuration.
/// </summary>
public class DefaultFallBackStartup
{
    /// <summary>
    ///     Configures the services.
    /// </summary>
    /// <param name="services">The service collection that manages registration of services.</param>
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<FallBackServiceHealth>(
                "default_health_check",
                HealthStatus.Degraded,
                new[] { "ready" });
    }

    /// <summary>
    ///     Configures the middleware registrations of the application.
    /// </summary>
    /// <param name="app">The app builder to be used.</param>
    public virtual void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapHealthChecks("/health");

                endpoints.MapHealthChecks(
                    "/health/ready",
                    new HealthCheckOptions
                    {
                        Predicate = check => check.Tags.Contains("ready")
                    });

                endpoints.MapHealthChecks(
                    "/health/live",
                    new HealthCheckOptions
                    {
                        Predicate = _ => true
                    });
            });
    }

    internal class FallBackServiceHealth : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Service is not configured properly and runs using a fallback startup."));
        }
    }
}
