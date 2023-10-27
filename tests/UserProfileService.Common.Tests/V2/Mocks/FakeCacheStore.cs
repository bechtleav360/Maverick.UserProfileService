using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.Tests.V2.Mocks
{
    public class FakeCacheStore : ICacheStore
    {
        public Dictionary<string, object> InternalCache { get; }

        public FakeCacheStore(Dictionary<string, object> internalCache)
        {
            InternalCache = internalCache;
        }

        public Task SetAsync<T>(
            string key,
            T obj,
            int expirationTime = 0,
            ICacheTransaction transaction = default,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            if (InternalCache.ContainsKey(key))
            {
                InternalCache.Remove(key);
            }

            InternalCache.Add(key, obj);

            return Task.FromResult(obj);
        }

        public Task<T> GetAsync<T>(
            string key,
            ICacheTransaction transaction = default,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            if (!InternalCache.TryGetValue(key, out object result)
                || result == null
                || result.GetType() != typeof(T))
            {
                return Task.FromResult<T>(default);
            }

            return Task.FromResult((T)result);
        }

        public Task<Tuple<bool, string>> LockAsync(
            string key,
            int expirationTime = 0,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LockReleaseAsync(string key, string lockId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(
            string key,
            ICacheTransaction transaction = default,
            CancellationToken token = default)
        {
            if (!InternalCache.ContainsKey(key))
            {
                return Task.CompletedTask;
            }

            InternalCache.Remove(key);

            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(
            string key,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            return GetAsync<T>(key, null, null, token);
        }

        public async Task DeleteAsync(IEnumerable<string> keys, CancellationToken token = default)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            foreach (string key in keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                await DeleteAsync(key, null, CancellationToken.None);
            }
        }
    }
}
