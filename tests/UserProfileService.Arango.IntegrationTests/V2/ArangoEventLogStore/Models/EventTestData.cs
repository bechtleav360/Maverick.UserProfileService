using UserProfileService.Common.V2.Enums;

namespace UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models
{
    public class EventTestData
    {
        public string Id { get; set; }

        public EventStatus Status { get; set; }

        public EventTestData(string id, EventStatus status = EventStatus.Initialized)
        {
            Id = id;
            Status = status;
        }
    }
}
