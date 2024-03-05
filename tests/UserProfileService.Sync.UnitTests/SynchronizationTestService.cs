using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Messaging.ArangoDb.Saga;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Services;
using UserProfileService.Sync.States;

namespace UserProfileService.Sync.UnitTests;

internal class SynchronizationTestService : SynchronizationService
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public Expression<Func<ProcessState, object>> SortExpression { get; set; }
    public SortOrder SortOrder { get; set; }
    public Guid IdOfLoadedEntity { get; set; }

    public SynchronizationTestService(
        IBus bus,
        ILogger<SynchronizationService> logger,
        IOptions<SyncConfiguration> syncOptions,
        IScheduleService scheduleService,
        ISyncProcessSynchronizer synchronizer,
        ISagaRepositoryQueryContextFactory<ProcessState> sagaRepositoryContextFactory,
        IMapper mapper) : base(
        bus,
        logger,
        scheduleService,
        synchronizer,
        sagaRepositoryContextFactory,
        syncOptions,
        mapper)
    {
    }

    /// <summary>
    /// <inheritdoc cref="SynchronizationService"/>
    /// </summary>
    protected internal override Task<Tuple<int, IList<ProcessState>>> ExecuteQueryContextAsync(
        int limit,
        int offset,
        Expression<Func<ProcessState, object>> sortExpression,
        Expression<Predicate<ProcessState>> filterExpression,
        SortOrder sortOrder,
        CancellationToken cancellationToken)
    {
        Limit = limit;
        Offset = offset;
        SortExpression = sortExpression;
        SortOrder = sortOrder;
        return base.ExecuteQueryContextAsync(limit, offset, sortExpression,filterExpression, sortOrder, cancellationToken);
    }


    /// <summary>
    /// <inheritdoc cref="SynchronizationService"/>
    /// </summary>
    protected internal override Task<ProcessState> LoadProcessStateFromContextAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        IdOfLoadedEntity = id;
        return base.LoadProcessStateFromContextAsync(id, cancellationToken);
    }
}