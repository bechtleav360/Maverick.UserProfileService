using System.Collections.Generic;
using UserProfileService.Common.V2.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models
{
    public class EventBatchTestData
    {
        public string Id { get; set; }

        public EventStatus Status { get; set; }

        public ICollection<EventTestData> EventTestData { get; set; }

        public EventBatchTestData(
            string id,
            EventStatus status = EventStatus.Initialized,
            params EventTestData[] eventTestData)
        {
            Id = id;
            Status = status;
            EventTestData = eventTestData;
        }
    }
}
