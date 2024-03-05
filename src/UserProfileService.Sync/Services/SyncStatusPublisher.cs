using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Responses;

namespace UserProfileService.Sync.Services;

/// <summary>
/// Represents a class responsible for periodically sending synchronization status updates.
/// </summary>
public class SyncStatusPublisher : BackgroundService
{
    private readonly IBus _bus;
    private readonly SyncConfiguration _syncConfiguration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncStatusPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncStatusPublisher"/> class.
    /// </summary>
    /// <param name="bus">The messaging bus used for communication.</param>
    /// <param name="syncConfiguration">The configuration settings for synchronization.</param>
    /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
    /// <param name="logger">The logger instance for logging synchronization status.</param>
    public SyncStatusPublisher(
        IBus bus,
        IOptions<SyncConfiguration> syncConfiguration,
        IServiceProvider serviceProvider,
        ILogger<SyncStatusPublisher> logger)
    {
        _bus = bus;
        _syncConfiguration = syncConfiguration.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }


    /// <summary>
    /// Asynchronously executes the synchronization process in the background.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.EnterMethod();

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInfoMessage("Getting status from the UPS-Sync", LogHelpers.Arguments());

            try
            {
                SyncStatus status = await GetSyncStatusAsync(stoppingToken);
                await _bus.Publish(status, stoppingToken);

                _logger.LogInfoMessage(
                    "UPS-Sync status (IsRunning: {status}) has been published successfully",
                    LogHelpers.Arguments(status.IsRunning.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogErrorMessage(
                    ex,
                    "Error happened by loading/sending UPS-Sync status",
                    LogHelpers.Arguments());
            }

            await Task.Delay(_syncConfiguration.LockExpirationTime * 1000 * 60, stoppingToken);
        }

        _logger.ExitMethod();
    }

    private async Task<SyncStatus> GetSyncStatusAsync(CancellationToken stoppingToken)
    {
        _logger.EnterMethod();

        using IServiceScope scope = _serviceProvider.CreateScope();
        var synchronizationService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();
        SyncStatus status = await synchronizationService.GetSyncStatusAsync(null, stoppingToken);

        return _logger.ExitMethod(status);
    }
}