using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Redis.Configuration;

namespace UserProfileService.Redis;

/// <summary>
///     Redis implementation of <see cref="ITempStore" />
/// </summary>
public class RedisTempObjectStore : RedisStoreBase, ITempStore
{
    /// <summary>
    ///     Create an instance of <see cref="RedisTempObjectStore" />.
    /// </summary>
    /// <param name="connectionMultiplexer">
    ///     <see cref="IConnectionMultiplexer" />
    /// </param>
    /// <param name="redisOptions">Options to use to configure redis and key-value handling.</param>
    /// <param name="logger">The logger.</param>
    public RedisTempObjectStore(
        IConnectionMultiplexer connectionMultiplexer,
        IOptionsMonitor<RedisConfiguration> redisOptions,
        ILogger<RedisTempObjectStore> logger)
        : base(connectionMultiplexer, redisOptions, logger)
    {
    }

    /// <inheritdoc />
    public async Task<IList<T>> GetListAsync<T>(
        string key,
        long? start = null,
        long? end = null,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        try
        {
            RedisValue[] values =
                start != null && end != null
                    ? await Store.ListRangeAsync(key, start.Value, end.Value)
                    : await Store.ListRangeAsync(key);

            Logger.LogDebugMessage("GetListAsync key {key}", LogHelpers.Arguments(key));

            if (values != null)
            {
                return values
                    .Where(v => !v.IsNullOrEmpty)
                    .Select(v => DeserializeSafely<T>(v, jsonSerializerSettingsProvider?.GetNewtonsoftSettings()))
                    .ToList();
            }
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(
                ex,
                "An error occurred while getting the list of key {key}.",
                LogHelpers.Arguments(key));
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<long> GetListLengthAsync<T>(string key, CancellationToken token = default)
    {
        try
        {
            long length = await Store.ListLengthAsync(key);
            Logger.LogDebugMessage("GetListAsync key {key}", LogHelpers.Arguments(key));

            return length;
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(
                ex,
                "An error occurred while getting the list of key {key}.",
                LogHelpers.Arguments(key));
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<IList<T>> GetAsync<T>(
        ISet<string> keys,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        JsonSerializerSettings serializerSettings = jsonSerializerSettingsProvider?.GetNewtonsoftSettings() != null
            ? jsonSerializerSettingsProvider.GetNewtonsoftSettings()
            : new JsonSerializerSettings();

        RedisKey[] redisKeys = keys
            .Select(k => new RedisKey(k))
            .ToArray();

        string joinedKeys = string.Join(" , ", keys);

        try
        {
            RedisValue[] values = await Store.StringGetAsync(redisKeys);
            Logger.LogDebugMessage("GetAsync keys {joinedKeys} in store.", LogHelpers.Arguments(joinedKeys));

            if (values != null)
            {
                return values
                    .Select(
                        v =>
                            v.IsNullOrEmpty
                                ? default
                                : JsonConvert.DeserializeObject<T>(v.ToString(), serializerSettings))
                    .ToList();
            }
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(
                ex,
                "An error occurred while getting keys {joinedKeys}.",
                LogHelpers.Arguments(joinedKeys));
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<long?> IncrementAsync(string key, int value = 1, CancellationToken token = default)
    {
        try
        {
            long storeValue = await Store.StringIncrementAsync(key, value);

            Logger.LogDebugMessage(
                "IncrementAsync key '{key}' to value {storeValue} in store.",
                LogHelpers.Arguments(key, storeValue));

            return storeValue;
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(ex, "An error occurred while increment key '{key}'", LogHelpers.Arguments(key));
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<long?> AddAsync<T>(
        string key,
        T add,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        try
        {
            RedisValue value = jsonSerializerSettingsProvider?.GetNewtonsoftSettings() != null
                ? JsonConvert.SerializeObject(add, jsonSerializerSettingsProvider.GetNewtonsoftSettings())
                : JsonConvert.SerializeObject(add);

            long storeValue = await Store.ListRightPushAsync(key, value);
            Logger.LogDebugMessage("AddAsync object to list with key '{key}'.", LogHelpers.Arguments(key));

            return storeValue;
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogError(ex, $"An error occurred while increment key '{key}'", LogHelpers.Arguments(key));
        }

        return null;
    }

    /// <inheritdoc />
    public Task<ICacheTransaction> CreateTransactionAsync(CancellationToken token = default)
    {
        return Task.FromResult<ICacheTransaction>(new RedisTransaction(Store.CreateTransaction()));
    }

    /// <inheritdoc />
    public async Task<long?> AddListAsync<T>(
        string key,
        IEnumerable<T> add,
        int expirationTime = 0,
        ICacheTransaction transactionObject = default,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        try
        {
            JsonSerializerSettings serializerSettings =
                jsonSerializerSettingsProvider?.GetNewtonsoftSettings() != null
                    ? jsonSerializerSettingsProvider.GetNewtonsoftSettings()
                    : new JsonSerializerSettings();

            RedisValue[] addArray = add?
                .Select(o => (RedisValue)JsonConvert.SerializeObject(o, serializerSettings))
                .ToArray();

            if (addArray == null || addArray.Length == 0)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            if (transactionObject is RedisTransaction { Payload: ITransaction transaction } redisTransaction)
            {
                redisTransaction.Operations.Add(transaction.ListRightPushAsync(key, addArray));

                token.ThrowIfCancellationRequested();

                redisTransaction.Operations.Add(
                    transaction.KeyExpireAsync(
                        key,
                        expirationTime > 0
                            ? TimeSpan.FromSeconds(expirationTime)
                            : ConfiguredExpirationTime));

                Logger.LogDebugMessage(
                    "Stored list as key {key} in temp store ({count} elements).",
                    LogHelpers.Arguments(key, addArray.Length));

                return -1L;
            }

            ITransaction lonelyTransaction = Store.CreateTransaction();

            Task<long> storageTask = Store.ListRightPushAsync(key, addArray);

            Task<bool> expirationTask = Store.KeyExpireAsync(
                key,
                expirationTime > 0
                    ? TimeSpan.FromSeconds(expirationTime)
                    : ConfiguredExpirationTime);

            if (await lonelyTransaction.ExecuteAsync())
            {
                long stored = await storageTask;
                await expirationTask;

                return stored;
            }

            return default;
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(ex, "An error occurred while increment key '{key}'", LogHelpers.Arguments(key));
        }

        return default;
    }
}
