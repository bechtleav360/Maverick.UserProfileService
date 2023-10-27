using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Health.Configuration;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.Health.Services;

/// <summary>
///     Implements an <see cref="IHostedService" /> which executes all flagged health-checks regularly and stores the
///     result.
/// </summary>
internal class ScheduledHealthCheckService : BackgroundService
{
    private readonly ScheduledHealthCheckConfiguration _configuration;
    private readonly HealthCheckService _healthChecks;
    private readonly ILogger<ScheduledHealthCheckService> _logger;
    private readonly IHealthStore _statusStore;

    /// <summary>
    ///     Initializes a new instance of <see cref="ScheduledHealthCheckService" />.
    /// </summary>
    /// <param name="healthChecks">The health check service to use.</param>
    /// <param name="statusStore">The status store to save results in.</param>
    /// <param name="logger">A <see cref="ILogger{TCategoryName}" /> used for logging.</param>
    /// <param name="configuration">A <see cref="ScheduledHealthCheckConfiguration" /> used to configure the instance.</param>
    public ScheduledHealthCheckService(
        HealthCheckService healthChecks,
        IHealthStore statusStore,
        ILogger<ScheduledHealthCheckService> logger,
        ScheduledHealthCheckConfiguration configuration)
    {
        _logger = logger;
        _statusStore = statusStore;
        _healthChecks = healthChecks;
        _configuration = configuration;
    }

    private async Task ScheduleHealthChecks(CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        _logger.LogDebugMessage("Executing scheduled health checks.", LogHelpers.Arguments());

        HealthReport healthReport =
            await _healthChecks.CheckHealthAsync(_configuration.FilterPredicate, cancellationToken);

        foreach (KeyValuePair<string, HealthReportEntry> entry in healthReport.Entries)
        {
            _statusStore.SetHealthStatus(
                entry.Key,
                new HealthState(
                    entry.Value.Status,
                    DateTime.UtcNow,
                    entry.Value.Description,
                    entry.Value.Exception));
        }

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

        // waiting to warmup
        await Task.Delay(TimeSpan.FromSeconds(1.5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebugMessage("Reporting health status via queue", LogHelpers.Arguments());

            try
            {
                await ScheduleHealthChecks(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogWarnMessage(
                    e,
                    "An error occurred while executing scheduled health-checks",
                    LogHelpers.Arguments());
            }

            await Task.Delay(_configuration.Delay, stoppingToken);
        }

        _logger.ExitMethod();
    }
}
