using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.V2.Abstractions;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace UserProfileService.Common.V2.Implementations;

/// <summary>
///     A implementation of <see cref="ITempStore" /> that stores everything in memory. It should be registered as
///     singleton.
/// </summary>
public class MemoryCacheImplementation : ITempStore
{
    private static readonly SemaphoreSlim _Lock = new SemaphoreSlim(1, 1);

    private static readonly Dictionary<string, Tuple<string, DateTimeOffset>> _LockingCollections
        = new Dictionary<string, Tuple<string, DateTimeOffset>>(StringComparer.OrdinalIgnoreCase);

    private readonly IMemoryCache _Store;

    /// <summary>
    ///     Initializes a new instance of <see cref="MemoryCacheImplementation" />.
    /// </summary>
    /// <param name="loggerFactory"></param>
    public MemoryCacheImplementation(ILoggerFactory loggerFactory)
    {
        _Store = new MemoryCache(
            Options.Create(new MemoryCacheOptions()),
            loggerFactory);
    }

    private TElem GetItem<TElem>(string key)
    {
        return _Store.TryGetValue(key, out object cItem) && cItem is TElem converted
            ? converted
            : default;
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task SetAsync<T>(
        string key,
        T obj,
        int expirationTime = 0,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        return Task.FromResult(_Store.Set(key, obj, TimeSpan.FromSeconds(expirationTime)));
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task<T> GetAsync<T>(
        string key,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        return Task.FromResult(
            _Store.TryGetValue(key, out T stored)
                ? stored
                : default);
    }

    /// <inheritdoc cref="ICacheStore" />
    public async Task<Tuple<bool, string>> LockAsync(
        string key,
        int expirationTime = 0,
        CancellationToken token = default)
    {
        await _Lock.WaitAsync(token);

        try
        {
            if (!_LockingCollections.TryGetValue(key, out Tuple<string, DateTimeOffset> s))
            {
                _LockingCollections.Add(
                    key,
                    new Tuple<string, DateTimeOffset>(
                        Guid.NewGuid().ToString(),
                        DateTimeOffset.UtcNow.AddSeconds(expirationTime)));

                return new Tuple<bool, string>(true, key);
            }

            if (s.Item2 > DateTimeOffset.UtcNow)
            {
                return new Tuple<bool, string>(true, s.Item1);
            }

            _LockingCollections.Remove(key);

            return new Tuple<bool, string>(false, s.Item1);
        }
        finally
        {
            _Lock.Release();
        }
    }

    /// <inheritdoc cref="ICacheStore.LockReleaseAsync" />
    public async Task<bool> LockReleaseAsync(string key, string lockId, CancellationToken token = default)
    {
        await _Lock.WaitAsync(token);

        try
        {
            if (!_LockingCollections.TryGetValue(key, out Tuple<string, DateTimeOffset> s) || s.Item1 != lockId)
            {
                return false;
            }

            _LockingCollections.Remove(key);

            return true;
        }
        finally
        {
            _Lock.Release();
        }
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task DeleteAsync(
        string key,
        ICacheTransaction transaction = default,
        CancellationToken token = default)
    {
        _Store.Remove(key);

        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task DeleteAsync(IEnumerable<string> keys, CancellationToken token = default)
    {
        foreach (string key in keys)
        {
            _Store.Remove(key);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ITempStore.GetListAsync{T}" />
    public Task<IList<T>> GetListAsync<T>(
        string key,
        long? start = null,
        long? end = null,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default)
    {
        IList<T> temp = _Store.TryGetValue(key, out object cItem) && cItem is IList<T> converted
            ? converted
            : new List<T>();

        if (start != null && end != null)
        {
            int length = (int)end.Value - (int)start.Value + 1;

            return Task.FromResult<IList<T>>(
                temp
                    .Skip((int)start.Value)
                    .Take(length)
                    .ToList());
        }

        return Task.FromResult(temp);
    }

    /// <inheritdoc cref="ITempStore.GetListLengthAsync{T}" />
    public Task<long> GetListLengthAsync<T>(string key, CancellationToken token = default)
    {
        IList<T> temp = GetItem<IList<T>>(key) ?? new List<T>();

        return Task.FromResult<long>(temp.Count);
    }

    /// <inheritdoc cref="ICacheStore.GetAsync{T}" />
    public Task<IList<T>> GetAsync<T>(
        ISet<string> keys,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default)
    {
        if (keys == null || keys.Count == 0)
        {
            return Task.FromResult<IList<T>>(new List<T>());
        }

        return Task.FromResult<IList<T>>(
            keys.Where(key => key != null)
                .Select(GetItem<T>)
                .Where(result => result != null)
                .ToList());
    }

    /// <inheritdoc cref="ITempStore.IncrementAsync" />
    public Task<long?> IncrementAsync(string key, int value = 1, CancellationToken token = default)
    {
        var number = GetItem<long?>(key);

        return Task.FromResult(number + value);
    }

    /// <inheritdoc cref="ITempStore.AddAsync{T}" />
    public async Task<long?> AddAsync<T>(
        string key,
        T add,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default)
    {
        await _Lock.WaitAsync(token);

        try
        {
            var list = GetItem<List<T>>(key);

            if (list == null)
            {
                list = new List<T>();
            }

            list.Add(add);

            _Store.Set(key, list);

            return list.Count;
        }
        finally
        {
            _Lock.Release();
        }
    }

    /// <inheritdoc cref="ITempStore.AddListAsync{T}" />
    public async Task<long?> AddListAsync<T>(
        string key,
        IEnumerable<T> add,
        int expirationTime = 0,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default)
    {
        List<T> toBeAdded = add as List<T> ?? add?.ToList() ?? new List<T>();

        await _Lock.WaitAsync(token);

        try
        {
            var list = GetItem<List<T>>(key);

            if (list == null)
            {
                list = new List<T>();
            }

            list.AddRange(toBeAdded);

            _Store.Set(key, list);

            return list.Count;
        }
        finally
        {
            _Lock.Release();
        }
    }

    /// <inheritdoc cref="ITempStore.CreateTransactionAsync" />
    public Task<ICacheTransaction> CreateTransactionAsync(CancellationToken token = default)
    {
        return Task.FromResult<ICacheTransaction>(new DeactivatedCacheTransaction());
    }
}
