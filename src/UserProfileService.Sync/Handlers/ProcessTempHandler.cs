using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Handlers;

/// <summary>
///     Implementation of <see cref="IProcessTempHandler" />
/// </summary>
public class ProcessTempHandler : IProcessTempHandler
{
    private readonly ITempStore _store;

    /// <summary>
    ///     Create an instance of <see cref="ProcessTempHandler" />.
    /// </summary>
    /// <param name="tempStore">Store to save entities temporary.</param>
    public ProcessTempHandler(
        ITempStore tempStore)
    {
        _store = tempStore;
    }

    private static string GenerateTempObjectsKey<TEntity>(string syncId)
    {
        
        string prefix = GeneratePrefix(syncId);

        string entityType = typeof(TEntity).GetCustomAttributeValue<ModelAttribute, string>(t => t.Model)
            .ToLowerInvariant();

        return $"{prefix}:{entityType}:temp";
    }

    private static string GenerateTempObjectKey(string syncId, Guid operationId)
    {
        string prefix = GeneratePrefix(syncId);

        return $"{prefix}:temp:{operationId}";
    }

    private static string GeneratePrefix(string syncId)
    {
        return $"ups:sync:{syncId}";
    }

    /// <inheritdoc />
    public async Task<IList<Guid>> GetTemporaryObjectKeysAsync<TEntity>(string syncId)
    {
        string tempsKey = GenerateTempObjectsKey<TEntity>(syncId);

        IList<Guid> tempKeys = await _store.GetListAsync<Guid>(tempsKey);

        return tempKeys;
    }

    /// <inheritdoc />
    public async Task<TEntity> GetTemporaryObjectAsync<TEntity>(string syncId, Guid operationId)
    {
        string storeKey = GenerateTempObjectKey(syncId, operationId);
        var tempObj = await _store.GetAsync<TEntity>(storeKey);

        return tempObj;
    }

    /// <inheritdoc />
    public async Task AddTemporaryObjectAsync<TEntity>(string syncId, Guid operationId, TEntity obj)
    {
        string storeKey = GenerateTempObjectKey(syncId, operationId);
        await _store.SetAsync(storeKey, obj);

        string tempsKey = GenerateTempObjectsKey<TEntity>(syncId);
        await _store.AddAsync(tempsKey, operationId);
    }
}
