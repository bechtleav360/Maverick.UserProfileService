using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Health.Configuration;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.Health.Services;

/// <summary>
///     A <see cref="IHostedService" /> which publishes the health-information regularly.
/// </summary>
public class PushHealthService : BackgroundService
{
    /// <summary>
    ///     The <see cref="IBus"/> used to publish the health check results.
    /// </summary>
    protected readonly IBus Bus;
    /// <summary>
    ///     Configuration used by this <see cref="PushHealthService"/>.
    /// </summary>
    protected readonly PushHealthCheckConfiguration Configuration;
    /// <summary>
    ///     <see cref="HealthCheckService"/> used to perform health checks.
    /// </summary>
    protected readonly HealthCheckService HealthCheckService;
    /// <summary>
    ///     Logger used by <see cref="PushHealthService"/>.
    /// </summary>
    protected readonly ILogger<PushHealthService> Logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="PushHealthStatus" />.
    /// </summary>
    /// <param name="healthCheckService">The <see cref="Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService" /> to use in order to fetch health information.</param>
    /// <param name="logger">Specifies which logger to use for logging.</param>
    /// <param name="configuration">The configuration used for configuring this instance.</param>
    /// <param name="bus">The <see cref="IBus" /> used to publish the health information.</param>
    public PushHealthService(
        HealthCheckService healthCheckService,
        ILogger<PushHealthService> logger,
        PushHealthCheckConfiguration configuration,
        IBus bus)
    {
        HealthCheckService = healthCheckService;
        Logger = logger;
        Configuration = configuration;
        Bus = bus;
    }

    /// <summary>
    ///     Publish the health status of the given worker/system as <see cref="HealthCheckMessage"/> to all subscribed consumers.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns> A <see cref="Task"/></returns>
    protected virtual async Task PushHealthStatus(CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage("Executing health checks to publish", LogHelpers.Arguments());

        HealthReport report =
            await HealthCheckService.CheckHealthAsync(Configuration.FilterPredicate, cancellationToken);

        var message = new HealthCheckMessage
        {
            InstanceName = Configuration.InstanceName,
            WorkerName = Configuration.WorkerName,
            Time = DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(Configuration.Delay.TotalMilliseconds * 1.5)),
            Status = report.Status,
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        };

        Logger.LogDebugMessage(
            "Publishing {Status} as health status via message broker",
            new object[] { message.Status });

        await Bus.Publish(message, cancellationToken);

        Logger.ExitMethod();
    }

    /// <summary>
    ///     Starts a long-running task which published the current health information regularly.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken" /> which can be used in order to stop the task.</param>
    /// <returns>A <see cref="Task" /> representing the long-running operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.EnterMethod();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Configuration.Delay, stoppingToken);

            Logger.LogDebugMessage("Reporting health status via queue", LogHelpers.Arguments());

            try
            {
                await PushHealthStatus(stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogWarnMessage(e, "An error occurred while pushing health checks", LogHelpers.Arguments());
            }
        }

        Logger.ExitMethod();
    }
}
