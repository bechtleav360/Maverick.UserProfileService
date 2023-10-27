using System;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Extensions;

/// <summary>
///     Contains some extensions method for the <see cref="StoreOptions" />
/// </summary>
public static class MartenStoreOptionsExtensions
{
    /// <summary>
    ///     Add the sync projection to marten db projection set.
    /// </summary>
    /// <param name="storeOptions"> Marten db store options <see cref="StoreOptions" /></param>
    /// <param name="serviceProvider">  The service provider <see cref="IServiceProvider" /></param>
    /// <param name="defaultSyncDaemonLockId">  The daemonLock Id used to identify the projection</param>
    /// <exception cref="ArgumentNullException"> Will be throw if one of the passed argument is null</exception>
    public static void AddSyncProjection(
        this StoreOptions storeOptions,
        IServiceProvider serviceProvider,
        int defaultSyncDaemonLockId = 5)
    {
        if (storeOptions == null)
        {
            throw new ArgumentNullException(nameof(storeOptions));
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        using IServiceScope scoped = serviceProvider.CreateScope();

        var syncProjection =
            scoped.ServiceProvider.GetRequiredService<ISyncProjection>();

        storeOptions.Projections.Add(
            (IProjection)syncProjection,
            ProjectionLifecycle.Async,
            "SyncProjection",
            _ => { storeOptions.Projections.DaemonLockId = defaultSyncDaemonLockId; });
    }
}
