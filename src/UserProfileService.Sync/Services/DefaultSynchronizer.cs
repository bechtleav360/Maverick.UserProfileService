using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Models;

namespace UserProfileService.Sync.Services;

/// <summary>
///     A default implementation of <see cref="ISyncProcessSynchronizer" />
/// </summary>
public class DefaultSynchronizer : ISyncProcessSynchronizer
{
    private readonly ILogger<DefaultSynchronizer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheStore _store;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Creates an instance of <see cref="DefaultSynchronizer" />.
    /// </summary>
    /// <param name="logger">   An instance of <see cref="ILogger" /></param>
    /// <param name="store">    An instance of <see cref="ICacheStore" /></param>
    /// <param name="provider"> An instance of <see cref="IServiceProvider" /> used to get objects from the IoC container</param>
    /// <param name="syncConfiguration">    Object containing the sync configuration</param>
    public DefaultSynchronizer(
        ILogger<DefaultSynchronizer> logger,
        IServiceProvider provider,
        ICacheStore store,
        IOptions<SyncConfiguration> syncConfiguration)
    {
        _logger = logger;
        _store = store;
        _serviceProvider = provider;
        _syncConfiguration = syncConfiguration.Value;
    }

    private async Task<bool> TrySetLockAsync(bool isReleased, CancellationToken token)
    {
        try
        {
            await _store.SetAsync(
                SyncConstants.SynchronizationKeys.SynLockObject,
                new SyncLock
                {
                    IsReleased = isReleased,
                    UpdatedAt = DateTime.Now
                },
                _syncConfiguration.LockExpirationTime * 60,
                token: token);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarnMessage(
                e,
                "Error happened by setting lock with key : {lockKey} for the UPS-Sync",
                LogHelpers.Arguments(SyncConstants.SynchronizationKeys.SynLockObject));

            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsSyncLockAvailableAsync(CancellationToken token = default)
    {
        _logger.EnterMethod();

        var syncLock = await _store.GetAsync<SyncLock>(SyncConstants.SynchronizationKeys.SynLockObject, token: token);

        return _logger.ExitMethod(syncLock == null || syncLock.IsReleased);
    }

    /// <inheritdoc />
    public async Task<bool> TryStartSync(StartSyncCommand startSyncCommand, CancellationToken token = default)
    {
        _logger.EnterMethod();

        if (startSyncCommand == null)
        {
            throw new ArgumentNullException(nameof(startSyncCommand));
        }

        using IServiceScope scope = _serviceProvider.CreateScope();
        var syncProcessCleaner = scope.ServiceProvider.GetRequiredService<ISyncProcessCleaner>();

        var syncLock = await _store.GetAsync<SyncLock>(SyncConstants.SynchronizationKeys.SynLockObject, token: token);

        var syncCanBeStarted = false;

        if (syncLock == null)
        {
            _logger.LogInfoMessage(
                "No Lock object existing for key: {key}, a new lock object will be created",
                LogHelpers.Arguments(SyncConstants.SynchronizationKeys.SynLockObject));

            syncCanBeStarted = await TrySetLockAsync(false, token);
        }
        else if (syncLock.IsReleased)
        {
            _logger.LogInfoMessage(
                "A released Lock object existing for key: {key}, the object will be lock used",
                LogHelpers.Arguments(SyncConstants.SynchronizationKeys.SynLockObject));

            syncCanBeStarted = await TrySetLockAsync(false, token);
        }

        if (syncCanBeStarted)
        {
            _logger.LogInfoMessage("Updating status of not completed sync processes", LogHelpers.Arguments());
            await syncProcessCleaner.UpdateAbortedProcessesStatusAsync(token);
        }

        return _logger.ExitMethod(syncCanBeStarted);
    }

    /// <inheritdoc />
    public async Task<bool> ReleaseLockForRunningProcessAsync(CancellationToken token = default)
    {
        _logger.EnterMethod();

        var syncLock = await _store.GetAsync<SyncLock>(SyncConstants.SynchronizationKeys.SynLockObject, token: token);

        if (syncLock == null)
        {
            return true;
        }

        bool releasedLock = await TrySetLockAsync(true, token);

        return _logger.ExitMethod(releasedLock);
    }
}
