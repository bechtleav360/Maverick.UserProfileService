using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Models.Results;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Systems;

/// <summary>
///     The Implementation of the synchronization for maverick as
///     source system.
/// </summary>
[System(SyncConstants.System.UserProfileService)]
public class MaverickSourceSystem : ISynchronizationSourceSystem<FunctionSync>
{
    private readonly ILogger<MaverickSourceSystem> _logger;
    private readonly IProfileService _profileService;

    /// <summary>
    ///     Create an instance of <see cref="LdapSourceSystem" />
    /// </summary>
    /// <param name="readService">The <see cref="IProfileService" /> to use in order to read mavericks data.</param>
    /// <param name="loggerFactory">
    ///     <see cref="ILoggerFactory" />
    /// </param>
    public MaverickSourceSystem(IProfileService readService, ILoggerFactory loggerFactory)
    {
        _profileService = readService;
        _logger = loggerFactory.CreateLogger<MaverickSourceSystem>();
    }

    /// <inheritdoc />
    public Task<FunctionSync> CreateEntity(FunctionSync entity, CancellationToken token)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public Task<FunctionSync> UpdateEntity(string sourceId, FunctionSync entity, CancellationToken token)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public Task DeleteEntity(string sourceId, CancellationToken token)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public async Task<IBatchResult<FunctionSync>> GetBatchAsync(int start, int batchSize, CancellationToken token)
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "Try to get functions with {start} (start) and {batchSize} (batchSize).",
            LogHelpers.Arguments(start, batchSize));

        IPaginatedList<FunctionSync> functions =
            await _profileService.GetFunctionsAsync<FunctionSync>(
                new AssignmentQueryObject
                {
                    Limit = batchSize,
                    Offset = start,
                    OrderedBy = nameof(FunctionSync.Name)
                },
                token);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTraceMessage(
                "Found {total} groups with limit {limit} and offset {offset}. Ids: {ids}.",
                LogHelpers.Arguments(
                    functions.TotalAmount,
                    batchSize,
                    start,
                    string.Join(" , ", functions.Select(p => p.Id))));
        }
        else
        {
            _logger.LogInfoMessage(
                "Found {total} groups for page {page} with limit {limit} and offset {offset}.",
                LogHelpers.Arguments(functions.TotalAmount, batchSize, start));
        }

        var result = new BatchResult<FunctionSync>
        {
            BatchSize = batchSize,
            CurrentPosition = start,
            StartedPosition = start,
            NextBatch = start + batchSize <= functions.TotalAmount,
            Result = functions.ToList()
        };

        _logger.ExitMethod();

        return result;
    }
}
