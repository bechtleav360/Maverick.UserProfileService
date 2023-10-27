using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Common.Tests.V2.Mocks
{
    public class FakeTempStore : FakeCacheStore, ITempStore
    {
        public FakeTempStore(Dictionary<string, object> internalCache) : base(internalCache)
        {
        }

        public Task<IList<T>> GetListAsync<T>(
            string key,
            long? start = null,
            long? end = null,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            IList<T> temp = InternalCache.TryGetValue(key, out object cItem) && cItem is IList<T> converted
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

        public Task<long> GetListLengthAsync<T>(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IList<T>> GetAsync<T>(
            ISet<string> keys,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long?> IncrementAsync(string key, int value = 1, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long?> AddAsync<T>(
            string key,
            T add,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<ICacheTransaction> CreateTransactionAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICacheTransaction>(default);
        }

        public Task<long?> AddListAsync<T>(
            string key,
            IEnumerable<T> add,
            int expirationTime = 0,
            ICacheTransaction transaction = default,
            IJsonSerializerSettingsProvider jsonSettingsProvider = default,
            CancellationToken token = default)
        {
            List<T> set = add as List<T> ?? add?.ToList();

            if (set == null)
            {
                throw new ArgumentNullException(nameof(add));
            }

            InternalCache.AddOrUpdate(key, set);

            return Task.FromResult<long?>(set.Count);
        }
    }
}
