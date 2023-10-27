using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Saga.Worker.Configuration;

namespace UserProfileService.Saga.Worker.Services;

internal class DatabaseCleanupBackgroundService : BackgroundService
{
    private readonly IDisposable _configChangedDisposable;
    private readonly object _configChangedLock = new object();
    private CleanupConfiguration _configuration;
    private readonly ILogger<DatabaseCleanupBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource _skipWaitingTokenSource;

    public DatabaseCleanupBackgroundService(
        IOptionsMonitor<CleanupConfiguration> optionsMonitor,
        ILogger<DatabaseCleanupBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        // to skip Task.Delay() during cleanup process, if configuration has been changed
        _skipWaitingTokenSource = new CancellationTokenSource();

        // just to determine difference of current and new state of config
        _configuration = optionsMonitor.CurrentValue;

        _configChangedDisposable = optionsMonitor.OnChange(OnConfigurationChange);
    }

    private void OnConfigurationChange(CleanupConfiguration newState)
    {
        lock (_configChangedLock)
        {
            if (_configuration != null && _configuration.Equals(newState))
            {
                return;
            }

            _configuration = newState;

            _logger.LogInfoMessage(
                "Configuration changed (new interval: {interval})",
                LogHelpers.Arguments(newState?.Interval));

            _skipWaitingTokenSource.Cancel(false);
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.EnterMethod();

        while (!stoppingToken.IsCancellationRequested)
        {
            lock (_configChangedLock)
            {
                if (_skipWaitingTokenSource == null
                    || _skipWaitingTokenSource.IsCancellationRequested)
                {
                    _skipWaitingTokenSource = new CancellationTokenSource();
                }
            }

            using IServiceScope serviceScope = _serviceProvider.CreateScope();

            // get a fresh configuration and avoid a difficult locking mechanism in this case
            TimeSpan interval = serviceScope.ServiceProvider
                .GetRequiredService<IOptionsSnapshot<CleanupConfiguration>>()
                .Value
                .Interval;

            _logger.LogInfoMessage(
                "Doing a cleanup of databases (background job)",
                LogHelpers.Arguments());

            foreach (IDatabaseCleanupProvider cleanupProvider in serviceScope.ServiceProvider
                         .GetServices<IDatabaseCleanupProvider>())
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogTraceMessage(
                        "Cancellation requested - skipping method",
                        LogHelpers.Arguments());

                    _logger.ExitMethod();

                    return;
                }

                try
                {
                    _logger.LogTraceMessage(
                        "Starting cleanup using provider: {cleanupProviderType}",
                        LogHelpers.Arguments(cleanupProvider.GetType().Name));

                    await cleanupProvider.CleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogTraceMessage(
                        "Cancellation requested - skipping method",
                        LogHelpers.Arguments());

                    _logger.ExitMethod();

                    return;
                }
                catch (Exception e)
                {
                    _logger.LogErrorMessage(
                        e,
                        "Error occurred during cleanup process of provider {cleanupProviderType}",
                        LogHelpers.Arguments(cleanupProvider.GetType().Name));
                }
            }

            try
            {
                var linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
                    stoppingToken,
                    _skipWaitingTokenSource.Token);

                _logger.LogInfoMessage(
                    "Cleanup finished - will wait {interval} till next cleanup",
                    LogHelpers.Arguments(interval));

                await Task.Delay(interval, linkedCancellationSource.Token);
            }
            // can happen if the configuration has been changed
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _configChangedDisposable?.Dispose();
    }
}
