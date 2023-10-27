using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Models;

namespace UserProfileService.Sync.Services;

/// <summary>
///     Default implementation of <see cref="ISyncProcessCleaner" />
/// </summary>
public class SyncProcessCleaner : ISyncProcessCleaner
{
    private readonly ILogger<SyncProcessCleaner> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create an instance of <see cref="SyncProcessCleaner" />
    /// </summary>
    /// <param name="logger"> A logger instance</param>
    /// <param name="provider">  An instance of <see cref="IServiceProvider" /></param>
    public SyncProcessCleaner(ILogger<SyncProcessCleaner> logger, IServiceProvider provider)
    {
        _logger = logger;
        _serviceProvider = provider;
    }

    /// <inheritdoc />
    public async Task UpdateAbortedProcessesStatusAsync(CancellationToken token = default)
    {
        _logger.EnterMethod();

        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var synchronizationService = scope.ServiceProvider.GetRequiredService<ISynchronizationService>();

            _logger.LogInfoMessage("Getting not completed sync processes", LogHelpers.Arguments());
            IEnumerable<ProcessView> activeProcesses = await synchronizationService.GetRunningSyncProcessAsync(token);

            foreach (ProcessView activeProcess in activeProcesses)
            {
                await synchronizationService.DeclareProcessAbortedAsync(activeProcess.CorrelationId, token);
            }
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by clearing not completed sync processes",
                LogHelpers.Arguments());

            throw;
        }

        _logger.ExitMethod();
    }
}
