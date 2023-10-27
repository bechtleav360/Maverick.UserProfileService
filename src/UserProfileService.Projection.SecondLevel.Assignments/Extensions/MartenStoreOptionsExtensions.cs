using System;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.Utilities;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Extensions;

/// <summary>
///     Contains some extensions method for the <see cref="StoreOptions" />
/// </summary>
public static class MartenStoreOptionsExtensions
{
    /// <summary>
    ///     Add the second level projection to marten db projection set.
    /// </summary>
    /// <param name="storeOptions"> Marten db store options <see cref="StoreOptions" /></param>
    /// <param name="serviceProvider">  The service provider <see cref="IServiceProvider" /></param>
    /// <param name="defaultProjectionLockId">  The daemonLock Id used to identify the projection</param>
    /// <exception cref="ArgumentNullException"> Will be throw if one of the passed argument is null</exception>
    public static void AddSecondLevelAssignmentProjection(
        this StoreOptions storeOptions,
        IServiceProvider serviceProvider,
        ProjectionDaemonLockId defaultProjectionLockId = ProjectionDaemonLockId.SecondLevelAssignmentsDaemonLockId)
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

        var secondLevelAssignmentProjection =
            scoped.ServiceProvider.GetRequiredService<IAssignmentsSecondLevelProjection>();

        storeOptions.Projections.Add(
            (IProjection)secondLevelAssignmentProjection,
            ProjectionLifecycle.Async,
            ProjectionNameConstants.SecondLevelAssignmentsProjection,
            _ => { storeOptions.Projections.DaemonLockId = (int)defaultProjectionLockId; });
    }
}
