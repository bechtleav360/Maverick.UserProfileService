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
internal class PushHealthService : BackgroundService
{
    private readonly IBus _bus;
    private readonly PushHealthCheckConfiguration _configuration;
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<PushHealthService> _logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="PushHealthStatus" />.
    /// </summary>
    /// <param name="healthCheckService">The <see cref="HealthCheckService" /> to use in order to fetch health information.</param>
    /// <param name="logger">Specifies which logger to use for logging.</param>
    /// <param name="configuration">The configuration used for configuring this instance.</param>
    /// <param name="bus">The <see cref="IBus" /> used to publish the health information.</param>
    public PushHealthService(
        HealthCheckService healthCheckService,
        ILogger<PushHealthService> logger,
        PushHealthCheckConfiguration configuration,
        IBus bus)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
        _configuration = configuration;
        _bus = bus;
    }

    private async Task PushHealthStatus(CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        _logger.LogDebugMessage("Executing health checks to publish", LogHelpers.Arguments());

        HealthReport report =
            await _healthCheckService.CheckHealthAsync(_configuration.FilterPredicate, cancellationToken);

        var message = new HealthCheckMessage
        {
            InstanceName = _configuration.InstanceName,
            WorkerName = _configuration.WorkerName,
            Time = DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(_configuration.Delay.TotalMilliseconds * 1.5)),
            Status = report.Status,
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        };

        _logger.LogDebugMessage(
            "Publishing {Status} as health status via message broker",
            new object[] { message.Status });

        await _bus.Publish(message, cancellationToken);

        _logger.ExitMethod();
    }

    /// <summary>
    ///     Starts a long-running task which published the current health information regularly.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken" /> which can be used in order to stop the task.</param>
    /// <returns>A <see cref="Task" /> representing the long-running operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.EnterMethod();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_configuration.Delay, stoppingToken);

            _logger.LogDebugMessage("Reporting health status via queue", LogHelpers.Arguments());

            try
            {
                await PushHealthStatus(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogWarnMessage(e, "An error occurred while pushing health checks", LogHelpers.Arguments());
            }
        }

        _logger.ExitMethod();
    }
}
