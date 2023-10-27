using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Seeding
{
    internal class ArangoEventLogStoreSeedingService
    {
        private readonly IArangoDbClient _client;
        private readonly ModelBuilderOptions _modelBuilderOptions;

        internal ArangoEventLogStoreSeedingService(IArangoDbClient client, ModelBuilderOptions modelOptions)
        {
            _client = client;
            _modelBuilderOptions = modelOptions;
        }

        private async Task CreateBatchAsync(EventBatch eventBatch)
        {
            string collection = _modelBuilderOptions.GetCollectionName(typeof(EventBatch));

            JObject jObject = eventBatch.InjectDocumentKey(t => t.Id, _client.UsedJsonSerializerSettings);

            await _client.CreateDocumentAsync(collection, jObject);
        }

        private async Task CreateBatchEventAsync(EventLogTuple eventBatch)
        {
            string eventBatchCollection = _modelBuilderOptions.GetCollectionName(typeof(EventBatch));
            string eventLogTupleCollection = _modelBuilderOptions.GetCollectionName(typeof(EventLogTuple));

            JObject jObject = eventBatch.InjectDocumentKey(t => t.Id, _client.UsedJsonSerializerSettings);

            await _client.CreateDocumentAsync(eventLogTupleCollection, jObject);

            string edgeCollection = _modelBuilderOptions.GetRelation<EventBatch, EventLogTuple>().EdgeCollection;
            string fromKey = GenerateArangoKey(eventBatchCollection, eventBatch.BatchId);
            string toKey = GenerateArangoKey(eventLogTupleCollection, eventBatch.Id);

            await _client.CreateEdgeAsync(
                edgeCollection,
                fromKey,
                toKey,
                new CreateDocumentOptions
                {
                    Overwrite = true,
                    OverWriteMode = AOverwriteMode.Replace
                },
                null);
        }

        private string GenerateArangoKey(string collectionName, string id)
        {
            return $"{collectionName}/{id}";
        }

        private EventBatch GenerateEventBatch(string id = null, EventStatus status = EventStatus.Initialized)
        {
            var eventBatch = new EventBatch
            {
                Id = id ?? Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = status
            };

            return eventBatch;
        }

        private EventLogTuple GenerateEvent(
            string batchId,
            string id = null,
            EventStatus status = EventStatus.Initialized)
        {
            var eventLogTuple = new EventLogTuple(
                                    new TestEvent
                {
                    EventId = id ?? Guid.NewGuid().ToString()
                },
                                    "TargetStream")
            {
                Id = id,
                BatchId = batchId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = status
            };

            return eventLogTuple;
        }

        public async Task SeedData(ICollection<EventBatchTestData> eventBatchTestData)
        {
            foreach (EventBatchTestData eventBatchTest in eventBatchTestData)
            {
                EventBatch eventBatch = GenerateEventBatch(eventBatchTest.Id, eventBatchTest.Status);

                await CreateBatchAsync(eventBatch);

                foreach (EventTestData eventTest in eventBatchTest.EventTestData)
                {
                    EventLogTuple @event = GenerateEvent(eventBatchTest.Id, eventTest.Id, eventTest.Status);

                    await CreateBatchEventAsync(@event);
                }
            }
        }
    }
}
