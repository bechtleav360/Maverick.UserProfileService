using System;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models
{
    internal class TestEvent : IUserProfileServiceEvent
    {
        public string Type => nameof(TestEvent);

        public string EventId { get; set; }

        public EventMetaData MetaData { get; set; }

        public string Value { get; set; }

        public TestEvent(string eventId = null, string batchId = null)
        {
            EventId = eventId ?? Guid.NewGuid().ToString();

            MetaData = new EventMetaData
            {
                Batch = new EventBatchData
                {
                    Id = batchId
                }
            };
        }
    }
}
