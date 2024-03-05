using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Messaging.ArangoDb.Saga;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Messages.Responses;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.Models.Views;
using UserProfileService.Sync.States;

namespace UserProfileService.Sync.Services;

/// <summary>
///     The implementation of <see cref="ISynchronizationService" />.
/// </summary>
public class SynchronizationService : ISynchronizationService
{
    private readonly IBus _bus;
    private readonly ILogger<SynchronizationService> _logger;
    private readonly IMapper _mapper;
    private readonly ISagaRepositoryQueryContextFactory<ProcessState> _sagaRepositoryContextFactory;
    private readonly IScheduleService _scheduleService;
    private readonly ISyncProcessSynchronizer _synchronizer;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Create an instance of <see cref="SynchronizationService" />.
    /// </summary>
    /// <param name="bus">Component that communicates with the configured bus.</param>
    /// <param name="logger">The logger for logging purposes.</param>
    /// <param name="scheduleService">Service to manage sync schedule.</param>
    /// <param name="synchronizer"> The service used for process synchronization</param>
    /// <param name="sagaRepositoryContextFactory">Factory to create context to access <see cref="ProcessState" /> data.</param>
    /// <param name="syncConfiguration">Factory to create context to access <see cref="ProcessState" /> data.</param>
    /// <param name="mapper">The mapper.</param>
    public SynchronizationService(
        IBus bus,
        ILogger<SynchronizationService> logger,
        IScheduleService scheduleService,
        ISyncProcessSynchronizer synchronizer,
        ISagaRepositoryQueryContextFactory<ProcessState> sagaRepositoryContextFactory,
        IOptions<SyncConfiguration> syncConfiguration,
        IMapper mapper)
    {
        _bus = bus;
        _logger = logger;
        _scheduleService = scheduleService;
        _sagaRepositoryContextFactory = sagaRepositoryContextFactory;
        _synchronizer = synchronizer;
        _syncConfiguration = syncConfiguration.Value;
        _mapper = mapper;
    }


    private static Expression<Func<ProcessState, object>> GetSortExpression(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return default;
        }

        if (propertyName.Equals(nameof(ProcessView.FinishedAt), StringComparison.OrdinalIgnoreCase))
        {
            return c => c.Process.FinishedAt;
        }

        if (propertyName.Equals(nameof(ProcessView.LastActivity), StringComparison.OrdinalIgnoreCase))
        {
            return c => c.Process.UpdatedAt;
        }

        if (propertyName.Equals(nameof(ProcessView.Status), StringComparison.OrdinalIgnoreCase))
        {
            return c => c.Process.Status;
        }

        if (propertyName.Equals(nameof(ProcessView.Initiator.DisplayName), StringComparison.OrdinalIgnoreCase))
        {
            return c => c.Initiator.DisplayName;
        }

        if (propertyName.Equals(nameof(ProcessView.Initiator.Name), StringComparison.OrdinalIgnoreCase))
        {
            return c => c.Initiator.Name;
        }

        if (propertyName.Equals(nameof(ProcessView.StartedAt), StringComparison.OrdinalIgnoreCase))
        {
            return c => c.Process.StartedAt;
        }

        throw new ArgumentException($"No property found with the name {propertyName}");
    }

    /// <inheritdoc />
    public async Task<Guid> StartSynchronizationAsync(
        string correlationId,
        bool schedule,
        CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        SyncSchedule syncSchedule = await _scheduleService.GetScheduleAsync(cancellationToken);

        // If no schedule is found, the sync should be activated by default. 
        // If the schedule is disabled and a scheduler starts the sync process,
        // an error is written because it was actively disabled.
        if (syncSchedule.IsActive != true && schedule)
        {
            throw new InvalidOperationException(
                "Sync cannot be started by the scheduler because the scheduler has been actively disabled.");
        }

        var syncProcessId = Guid.NewGuid();

        _logger.LogInfoMessage(
            "Start synchronization with processId {processId}",
            LogHelpers.Arguments(syncProcessId));

        var message = new StartSyncCommand
        {
            CorrelationId = syncProcessId,
            StartedByScheduler = schedule,
            InitiatorId = SyncConstants.Initiator.Api
        };

        await _bus.Publish(message, cancellationToken);

        return _logger.ExitMethod(syncProcessId);
    }

    /// <inheritdoc />
    public async Task<ProcessView> GetProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identifier of process can not be empty", nameof(id));
        }

        ProcessState processState = await LoadProcessStateFromContextAsync(id, cancellationToken);

        var syncProcess = _mapper.Map<ProcessView>(processState);

        return _logger.ExitMethod(syncProcess);
    }

    /// <inheritdoc />
    public async Task<ProcessDetail> GetDetailedProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identifier of process can not be empty", nameof(id));
        }

        ProcessState processState = await LoadProcessStateFromContextAsync(id, cancellationToken);

        var syncProcess = _mapper.Map<ProcessDetail>(processState);

        return _logger.ExitMethod(syncProcess);
    }

    /// <inheritdoc />
    public async Task<SyncStatus> GetSyncStatusAsync(
        string requestId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage("Getting information about lock object status", LogHelpers.Arguments());

        bool isLockReleased = await _synchronizer.IsSyncLockAvailableAsync(cancellationToken);

        _logger.LogInfoMessage("Getting information about running sync process", LogHelpers.Arguments());

        IEnumerable<ProcessView> processes = await GetRunningSyncProcessAsync(cancellationToken);

        IEnumerable<ProcessView> processViews = processes as ProcessView[] ?? processes.ToArray();

        if (processViews.Any() != true && isLockReleased)
        {
            _logger.LogInfoMessage("No running sync processes, sync status will be returned", LogHelpers.Arguments());

            return
                new SyncStatus
                {
                    IsRunning = false,
                    RequestId = requestId
                };
        }

        if (isLockReleased) // process is running longer than 15 min (default value)
        {
            _logger.LogInfoMessage("Sync lock object is released, sync status will be returned ", LogHelpers.Arguments());

            if (processViews.Any())
            {
                bool areProcessesHanging = processViews.Any(
                    p => p.LastActivity != null
                         && DateTime.Compare(DateTime.UtcNow, p.LastActivity.Value.AddMinutes(_syncConfiguration.DelayBeforeTimeoutForStep)) > 0);

                return new SyncStatus
                {
                    IsRunning = !areProcessesHanging,
                    RequestId = requestId
                };
            }

            return new SyncStatus
            {
                IsRunning = false,
                RequestId = requestId
            };
        }

        _logger.LogInfoMessage("A synchronization is running, sync status will be returned", LogHelpers.Arguments());

        return new SyncStatus
        {
            IsRunning = true,
            RequestId = requestId,
            Process = processViews.FirstOrDefault()
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProcessView>> GetRunningSyncProcessAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        try
        {
            (int _, IList<ProcessState> states) = await _sagaRepositoryContextFactory.ExecuteQuery(
                t => t.QueryAsync(
                    100,
                    0,
                    GetSortExpression(nameof(ProcessView.StartedAt)),
                    p => p.Process.FinishedAt == null,
                    SortOrder.Asc,
                    cancellationToken),
                cancellationToken);

            List<ProcessView> processes = states.Where(process => process.Process != null && process.Version > 0)
                .Select(s => _mapper.Map<ProcessView>(s))
                .ToList();

            return _logger.ExitMethod(processes);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error happened getting the running sync processes");

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PaginatedList<ProcessView>> GetAllProcessesAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        (int count, IList<ProcessState> states) = await _sagaRepositoryContextFactory.ExecuteQuery(
            t => t.QueryAsync(pageSize, (page - 1) * pageSize, cancellationToken: cancellationToken),
            cancellationToken);

        var processes = _mapper.Map<IList<ProcessView>>(states);

        var result = new PaginatedList<ProcessView>(processes, count);

        return _logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public async Task<PaginatedList<ProcessView>> GetAllProcessesAsync(
        QueryObject queryObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        queryObject ??= new QueryObject();

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTraceMessage(
                "Try to get all sync processes with query: {query}.",
                LogHelpers.Arguments(JsonConvert.SerializeObject(queryObject)));
        }

        int page = queryObject.Page;
        int pageSize = queryObject.PageSize;

        _logger.LogInfoMessage(
            queryObject.OrderedBy != null
                ? "Trying to get all sync processes ordered by property: {orderedBy}"
                : "Trying to get all sync processes",
            LogHelpers.Arguments(queryObject.OrderedBy));

        Expression<Func<ProcessState, object>> sortExpression = GetSortExpression(queryObject.OrderedBy);

        (int count, IList<ProcessState> states) = await ExecuteQueryContextAsync(
            pageSize,
            (page - 1) * pageSize,
            sortExpression,
            null,
            queryObject.SortOrder,
            cancellationToken);

        var processes = _mapper.Map<IList<ProcessView>>(states);

        return new PaginatedList<ProcessView>(processes, count);
    }

    /// <inheritdoc />
    public async Task DeclareProcessAbortedAsync(Guid processId, CancellationToken token)
    {
        _logger.EnterMethod();

        await _sagaRepositoryContextFactory.ExecuteQuery(
            async t =>
            {
                var instance = await t.Load(processId);

                instance.Process ??= new Process();

                DateTime dateNow = DateTime.UtcNow;

                if (instance.Process.StartedAt == default)
                {
                    instance.Process.StartedAt = dateNow;
                }

                instance.Process.UpdatedAt = dateNow;
                instance.Process.Status = ProcessStatus.Aborted;
                instance.Process.FinishedAt = dateNow;

                await t.UpdateAsync(instance, token);

                return instance;
            },
            token);

        _logger.ExitMethod();
    }

    /// <summary>
    ///     Executes the given query on the <see cref="ISagaRepositoryQueryContextFactory{TSaga}" />
    /// </summary>
    /// <param name="limit">The number of result.</param>
    /// <param name="offset">The number of records that need to be skipped</param>
    /// <param name="sortExpression">The expression used to sort the result according to a defined property.</param>
    /// <param name="filterExpression">The expression used for filtering purposes according to a defined property.</param>
    /// <param name="sortOrder">The sorting order (ascending(ASC) or descending(DESC))</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns></returns>
    protected internal virtual async Task<Tuple<int, IList<ProcessState>>> ExecuteQueryContextAsync(
        int limit,
        int offset,
        Expression<Func<ProcessState, object>> sortExpression,
        Expression<Predicate<ProcessState>> filterExpression,
        SortOrder sortOrder,
        CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        return _logger.ExitMethod(
            await _sagaRepositoryContextFactory.ExecuteQuery(
                t => t.QueryAsync(
                    limit,
                    offset,
                    sortExpression,
                    filterExpression,
                    sortOrder,
                    cancellationToken),
                cancellationToken));
    }

    /// <summary>
    ///     Load an entity from a repository created through <see cref="ISagaRepositoryQueryContextFactory{TSaga}" />
    /// </summary>
    /// <param name="id">The of the entity that should be loaded.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A Task containing a <see cref="ProcessState" />.</returns>
    protected internal virtual async Task<ProcessState> LoadProcessStateFromContextAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        ProcessState processState = await _sagaRepositoryContextFactory.Execute(t => t.Load(id), cancellationToken);

        return _logger.ExitMethod(processState);
    }
}
