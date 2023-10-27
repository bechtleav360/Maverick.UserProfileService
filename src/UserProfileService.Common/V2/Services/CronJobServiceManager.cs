using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.Services;

/// <summary>
///     Represents a manager that will take care of time-triggered tasks.
/// </summary>
public class CronJobServiceManager : BackgroundService
{
    private readonly IActivitySourceWrapper _ActivitySourceWrapper;
    private readonly ILogger<CronJobServiceManager> _Logger;
    private readonly int _MinutesToWait = 5;
    private readonly IServiceProvider _ServiceProvider;

    /// <summary>
    ///     Initializes a new instance of <see cref="CronJobServiceManager" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider to retrieve DI containers.</param>
    /// <param name="logger">The logger to be used for logging messages.</param>
    /// <param name="activitySourceWrapper">A wrapper for activity sources.</param>
    public CronJobServiceManager(
        IServiceProvider serviceProvider,
        ILogger<CronJobServiceManager> logger,
        IActivitySourceWrapper activitySourceWrapper)
    {
        _ServiceProvider = serviceProvider;
        _Logger = logger;
        _ActivitySourceWrapper = activitySourceWrapper;
    }

    /// <inheritdoc cref="BackgroundService" />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _Logger.EnterMethod();

        while (!stoppingToken.IsCancellationRequested)
        {
            Activity activity = _ActivitySourceWrapper.ActivitySource?.StartActivity(
                $"RunningCronJob_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                ActivityKind.Producer);

            _Logger.LogInfoMessage(
                "Executing time-triggered tasks in all registered services.",
                LogHelpers.Arguments());

            using IServiceScope scope = _ServiceProvider.CreateScope();
            List<ICronJobService> cronJobServices = scope.ServiceProvider.GetServices<ICronJobService>().ToList();

            if (cronJobServices.Count == 0)
            {
                _Logger.LogInfoMessage(
                    "No service with time-triggered tasks registered. Skipping.",
                    LogHelpers.Arguments());

                _Logger.ExitMethod();

                return;
            }

            List<Task> tasks = cronJobServices
                .Select(s => Task.Run(() => s.ExecuteAsync(stoppingToken), stoppingToken))
                .ToList();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _Logger.LogErrorMessage(
                    ex,
                    "{n} of {total} time-triggered tasks produced an error.",
                    new object[] { tasks.Select(t => t.Status == TaskStatus.Faulted).Count(), tasks.Count });
            }

            _Logger.LogInfoMessage("Time-triggered tasks executed.", LogHelpers.Arguments());

            activity?.Stop();

            await Task.Delay(TimeSpan.FromMinutes(_MinutesToWait), stoppingToken);
        }

        _Logger.ExitMethod();
    }
}
