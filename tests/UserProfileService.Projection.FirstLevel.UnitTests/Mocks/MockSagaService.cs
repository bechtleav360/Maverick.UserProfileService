using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common;
using UserProfileService.Projection.Common.Abstractions;
using Xunit.Sdk;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Mocks
{
    public class MockSagaService : ISagaService
    {
        private readonly Dictionary<Guid, List<EventTuple>> _eventTupleDictionary =
            new Dictionary<Guid, List<EventTuple>>();

        private Guid _batchId;

        public Task AbortBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
        { 
            _eventTupleDictionary.Clear();

            return Task.CompletedTask;
        }

        public Task AddEventsAsync(
            Guid batchId,
            IEnumerable<EventTuple> events,
            CancellationToken cancellationToken = default)
        {
            if (events == null)
            {
                throw new XunitException("The events list is null");
            }

            EventTuple[] eventTuples = events as EventTuple[] ?? events.ToArray();

            if (!eventTuples.Any())
            {
                throw new XunitException("The event list is empty");
            }

            if (_eventTupleDictionary.Count == 0)
            {
                _eventTupleDictionary.Add(batchId, eventTuples.ToList());

                return Task.CompletedTask;
            }

            _eventTupleDictionary[batchId].AddRange(eventTuples);

            return Task.CompletedTask;
        }

        public async Task<Guid> CreateBatchAsync(CancellationToken cancellationToken = default)
        {
            _batchId = Guid.NewGuid();

            return await Task.FromResult(_batchId);
        }

        public async Task<Guid> CreateBatchAsync(CancellationToken cancellationToken, params EventTuple[] initialEvents)
        {
            if (initialEvents == null)
            {
                throw new XunitException(nameof(initialEvents));
            }

            _batchId = Guid.NewGuid();
            _eventTupleDictionary.Add(_batchId, initialEvents.ToList());

            return await Task.FromResult(_batchId);
        }

        public Task ExecuteBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public IReadOnlyDictionary<Guid, List<EventTuple>> GetDictionary()
        {
            return new ReadOnlyDictionary<Guid, List<EventTuple>>(_eventTupleDictionary);
        }
    }
}
